using ExcelDataReader;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        DAL dbLogic = new DAL();
        private BindingSource bindingSource = new BindingSource();

        public Form1()
        {
            InitializeComponent();

            // Pasang event handler untuk tombol yang belum terhubung di designer
            this.Load += Form1_Load;
            btnRekap.Click += btnRekap_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnCari.Click += btnCari_Click;
            // Tombol lain (button1-5, btnConnect, btnLoad, btnImport, btnDatabase, btnUpload) sudah terhubung di designer
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            LoadData();
            BindControls();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = dbLogic.GetMhs();
                bindingSource.DataSource = dt;
                dataGridView1.DataSource = bindingSource;

                if (dataGridView1.Columns["Foto"] != null)
                {
                    DataGridViewImageColumn fotoColumn = (DataGridViewImageColumn)dataGridView1.Columns["Foto"];
                    fotoColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;
                }

                HitungTotal();
                EnableControls(true);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void BindControls()
        {
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtKodeProdi.DataBindings.Clear();

            txtNIM.DataBindings.Add("Text", bindingSource, "NIM");
            txtNama.DataBindings.Add("Text", bindingSource, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin");
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat");
            txtKodeProdi.DataBindings.Add("Text", bindingSource, "KodeProdi");
        }

        // ==================== ENABLE CONTROLS ====================
        private void EnableControls(bool enabled)
        {
            dataGridView1.Enabled = enabled;

            // Tombol yang selalu aktif (kecuali sedang import)
            btnImport.Enabled = true;
            btnDatabase.Enabled = enabled;
            btnUpload.Enabled = enabled;
            btnConnect.Enabled = enabled;
            btnLoad.Enabled = enabled;
            btnRefresh.Enabled = enabled;
            btnCari.Enabled = enabled;

            // Tombol CRUD aktif hanya saat data ditampilkan
            button1.Enabled = enabled;   // Reset Data
            button2.Enabled = enabled;   // Test Injection
            btnRekap.Enabled = enabled;
            button3.Enabled = enabled;   // Insert
            button4.Enabled = enabled;   // Update
            button5.Enabled = enabled;   // Delete
        }

        // ==================== KONVERSI GAMBAR KE BYTE ====================
        private byte[] ImageToBytes(PictureBox pb)
        {
            if (pb.Image == null) return null;

            try
            {
                using (Bitmap bmp = new Bitmap(pb.Image))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] result = ms.ToArray();
                        if (result.Length > 0)
                        {
                            SimpanLog($"SUKSES: Gambar dikonversi ({result.Length} byte)");
                            return result;
                        }
                        else
                        {
                            SimpanLog("ERROR: Gambar hasil konversi 0 byte");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SimpanLog($"GAGAL KONVERSI GAMBAR: {ex.Message}");
                MessageBox.Show($"Gagal konversi gambar: {ex.Message}\nGambar akan diabaikan.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        // ==================== INSERT (button3) ====================
        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = ImageToBytes(pictureBox1);
                dbLogic.InsertMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text,
                                   dtpTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);
                MessageBox.Show("Data berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog("SQL INSERT ERROR: " + ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("GENERAL INSERT ERROR: " + ex.Message);
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ==================== UPDATE (FIX: FOTO TETAP ADA) ====================
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                string nim = txtNIM.Text.Trim();
                if (string.IsNullOrEmpty(nim))
                {
                    MessageBox.Show("NIM tidak boleh kosong!");
                    return;
                }

                // 1. Ambil foto lama dari database
                byte[] oldFoto = dbLogic.GetFotoByNIM(nim);

                // 2. Default: gunakan foto lama (jika ada)
                byte[] finalFoto = oldFoto;

                // 3. Jika pictureBox berisi gambar, coba konversi
                if (pictureBox1.Image != null)
                {
                    byte[] newFoto = ImageToBytes(pictureBox1);
                    if (newFoto != null && newFoto.Length > 0)
                    {
                        finalFoto = newFoto; // gunakan gambar baru
                        SimpanLog("Update: menggunakan gambar baru");
                    }
                    else
                    {
                        // Konversi gagal, tanyakan apakah mau tetap pakai gambar lama
                        if (oldFoto != null && oldFoto.Length > 0)
                        {
                            DialogResult dr = MessageBox.Show(
                                "Gagal mengkonversi gambar baru.\nGunakan gambar yang sudah ada?",
                                "Konfirmasi",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            if (dr == DialogResult.No)
                                finalFoto = null; // hapus gambar
                            else
                                finalFoto = oldFoto; // pertahankan
                        }
                        else
                        {
                            // Tidak ada gambar lama, tanyakan lanjut tanpa gambar
                            DialogResult dr = MessageBox.Show(
                                "Gagal mengkonversi gambar dan tidak ada gambar lama.\nLanjutkan update tanpa gambar?",
                                "Peringatan",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);
                            if (dr == DialogResult.No)
                                return;
                            else
                                finalFoto = null;
                        }
                    }
                }
                else
                {
                    // PictureBox kosong – tanyakan apakah ingin menghapus gambar lama
                    if (oldFoto != null && oldFoto.Length > 0)
                    {
                        DialogResult dr = MessageBox.Show(
                            "Anda belum mengupload gambar.\nHapus gambar yang ada?",
                            "Konfirmasi",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                            finalFoto = null;
                        else
                            finalFoto = oldFoto;
                    }
                    else
                        finalFoto = null;
                }

                // 4. Update database
                dbLogic.UpdateMhs(nim, txtNama.Text, txtAlamat.Text, cmbJK.Text,
                                  dtpTanggalLahir.Value.Date, txtKodeProdi.Text, finalFoto);

                MessageBox.Show("Data berhasil diupdate");

                // 5. Refresh tampilan
                LoadData();

                // 6. Tampilkan gambar hasil update
                if (finalFoto != null && finalFoto.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(finalFoto))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    SimpanLog($"Update: gambar ditampilkan ({finalFoto.Length} byte)");
                }
                else
                {
                    pictureBox1.Image = null;
                    SimpanLog("Update: gambar dikosongkan");
                }

                KosongkanTextboxSaja();
            }
            catch (SqlException ex)
            {
                SimpanLog("SQL UPDATE ERROR: " + ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("GENERAL UPDATE ERROR: " + ex.Message);
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ==================== TAMPILKAN GAMBAR DARI DATABASE ====================
        private void TampilkanGambarDariDatabase(string nim)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["NIM"].Value?.ToString() == nim)
                    {
                        object fotoValue = row.Cells["Foto"].Value;
                        if (fotoValue != DBNull.Value && fotoValue != null)
                        {
                            byte[] fotoBytes = (byte[])fotoValue;
                            if (fotoBytes.Length > 0)
                            {
                                using (MemoryStream ms = new MemoryStream(fotoBytes))
                                {
                                    Image img = Image.FromStream(ms);
                                    pictureBox1.Image = new Bitmap(img);
                                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                                    SimpanLog($"SUKSES: Gambar ditampilkan dari database ({fotoBytes.Length} byte)");
                                }
                            }
                            else
                            {
                                pictureBox1.Image = null;
                                SimpanLog("INFO: Gambar di database kosong (0 byte)");
                            }
                        }
                        else
                        {
                            pictureBox1.Image = null;
                            SimpanLog("INFO: Gambar di database NULL");
                        }
                        break;
                    }
                }
            }
            catch (Exception imgEx)
            {
                pictureBox1.Image = null;
                SimpanLog("GAGAL TAMPILKAN GAMBAR DARI DATABASE: " + imgEx.Message);
                MessageBox.Show("Gagal menampilkan gambar dari database: " + imgEx.Message);
            }
        }

        // ==================== KOSONGKAN TEXTBOX (TANPA MENGHAPUS GAMBAR) ====================
        private void KosongkanTextboxSaja()
        {
            txtNIM.Enabled = true;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            txtNIM.Focus();
        }

        // ==================== DELETE (button5) ====================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dg = MessageBox.Show("Hapus data ini?", "Konfirmasi",
                                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dg == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(txtNIM.Text);
                    MessageBox.Show("Data berhasil dihapus");
                    ClearForm();
                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                SimpanLog("SQL DELETE ERROR: " + ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("GENERAL DELETE ERROR: " + ex.Message);
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ==================== CLEAR FORM (UNTUK INSERT & DELETE) ====================
        private void ClearForm()
        {
            txtNIM.Enabled = true;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            pictureBox1.Image = null;
            txtNIM.Focus();
        }

        // ==================== TOMBOL LAINNYA ====================
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DAL.GetConnectionString()))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi berhasil!");
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Koneksi gagal: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e) { LoadData(); }

        // ==================== RESET DATA (button1) ====================
        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset");
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ==================== TEST INJECTION (button2) ====================
        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.testInject(txtNIM.Text);
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQLDebugging Error: unsafe UPDATE operation not allowed");
                }
                else
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQLDebugging Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("GENERAL ERROR: " + ex.Message);
            }
        }

        // ==================== CARI ====================
        private void btnCari_Click(object sender, EventArgs e)
        {
            string nim = txtNIM.Text.Trim();
            if (!string.IsNullOrEmpty(nim))
            {
                try
                {
                    DataTable dt = dbLogic.GetMhsByNIM(nim);
                    bindingSource.DataSource = dt;
                    dataGridView1.DataSource = bindingSource;
                }
                catch (Exception ex)
                {
                    SimpanLog("GAGAL CARI: " + ex.Message);
                    MessageBox.Show("Gagal mencari data: " + ex.Message);
                }
            }
            else
            {
                LoadData();
            }
        }

        // ==================== REFRESH ====================
        private void btnRefresh_Click(object sender, EventArgs e) { LoadData(); }

        // ==================== REKAP ====================
        private void btnRekap_Click(object sender, EventArgs e)
        {
            RekapMahasiswa frmRekap = new RekapMahasiswa("", DateTime.Now);
            frmRekap.Show();
        }

        // ==================== HITUNG TOTAL ====================
        private void HitungTotal()
        {
            try
            {
                int total = dbLogic.CountMhs();
                lblTotal.Text = "Total Mahasiswa : " + total;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal hitung total: " + ex.Message);
            }
        }

        private void SimpanLog(string message)
        {
            dbLogic.InsertLog(message);
        }

        // ==================== DATA GRID VIEW – KLIK BARIS ====================
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataRowView rowView = (DataRowView)bindingSource[e.RowIndex];
                DataRow row = rowView.Row;

                txtNIM.Text = row["NIM"].ToString();
                txtNama.Text = row["Nama"].ToString();
                cmbJK.Text = row["JenisKelamin"].ToString();
                dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);
                txtAlamat.Text = row["Alamat"].ToString();
                txtKodeProdi.Text = row["KodeProdi"].ToString();

                if (row["Foto"] != DBNull.Value && row["Foto"] != null)
                {
                    try
                    {
                        byte[] imgBytes = (byte[])row["Foto"];
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            Image img = Image.FromStream(ms);
                            pictureBox1.Image = new Bitmap(img);
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        SimpanLog($"Gambar dimuat ({imgBytes.Length} byte)");
                    }
                    catch (Exception imgEx)
                    {
                        pictureBox1.Image = null;
                        SimpanLog("GAGAL MUAT GAMBAR: " + imgEx.Message);
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                    SimpanLog("INFO: Tidak ada gambar untuk NIM " + txtNIM.Text);
                }
            }
        }

        // ==================== UPLOAD GAMBAR ====================
        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image img = Image.FromFile(ofd.FileName);
                    pictureBox1.Image = new Bitmap(img);
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    SimpanLog($"Gambar diupload dari: {ofd.FileName}");
                }
                catch (Exception ex)
                {
                    SimpanLog("GAGAL UPLOAD GAMBAR: " + ex.Message);
                    MessageBox.Show("Gagal upload gambar: " + ex.Message);
                }
            }
        }

        // ==================== IMPORT EXCEL ====================
        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Workbook|*.xls;*.xlsx" })
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = true
                                }
                            });
                            DataTable dt = result.Tables[0];
                            dataGridView1.DataSource = dt;
                            dataGridView1.Enabled = false;

                            // Nonaktifkan tombol lain saat mode import
                            btnImport.Enabled = true;
                            btnDatabase.Enabled = true;
                            button1.Enabled = false;
                            button2.Enabled = false;
                            btnRekap.Enabled = false;
                            btnRefresh.Enabled = false;
                            btnCari.Enabled = false;
                            btnLoad.Enabled = false;
                            button3.Enabled = false;
                            button4.Enabled = false;
                            button5.Enabled = false;
                            btnUpload.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal import Excel: " + ex.Message);
                SimpanLog("IMPORT EXCEL ERROR: " + ex.Message);
            }
        }

        // ==================== IMPORT TO DATABASE ====================
        private void btnDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string kodeProdi = row["KodeProdi"].ToString().Trim();
                    string fotoPath = row.Table.Columns.Contains("FotoPath") ? row["FotoPath"].ToString().Trim() : "";

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama))
                        continue;

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir))
                        continue;

                    byte[] fotoBytes = null;
                    if (!string.IsNullOrEmpty(fotoPath) && File.Exists(fotoPath))
                        fotoBytes = File.ReadAllBytes(fotoPath);

                    dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);
                    sukses++;
                }
                MessageBox.Show($"{sukses} data mahasiswa berhasil diimport.");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog("SQL IMPORT ERROR: " + ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("GENERAL IMPORT ERROR: " + ex.Message);
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
}