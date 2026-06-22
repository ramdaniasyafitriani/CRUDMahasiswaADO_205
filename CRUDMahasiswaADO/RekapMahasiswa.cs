using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CRUDMahasiswaADO
{
    public partial class RekapMahasiswa : Form
    {
        // Hapus static connectionString, kita pakai local variable di konstruktor
        private SqlConnection conn;
        private SqlDataAdapter da;
        private DataTable dtMahasiswa;

        string prodi { get; set; }
        DateTime tglmasuk { get; set; }

        public RekapMahasiswa(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();

            // Buat koneksi langsung di sini
            string connectionString = "Data Source=LAPTOP-M4FI536S\\RAMDANIAA;Initial Catalog=DBAkademikADO;Integrated Security=True";
            conn = new SqlConnection(connectionString);

            this.prodi = Prodi;
            this.tglmasuk = TglMasuk;

            // ============================================
            // 1. SETTING KONTROL
            // ============================================
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            cmbProdi.DropDownStyle = ComboBoxStyle.DropDownList;

            // ============================================
            // 2. ISI COMBOBOX
            // ============================================
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT NamaProdi FROM ProgramStudi", conn);
                DataTable dtProdiLocal = new DataTable();
                SqlDataAdapter daLocal = new SqlDataAdapter(cmd);
                daLocal.Fill(dtProdiLocal);

                if (dtProdiLocal.Rows.Count > 0)
                {
                    cmbProdi.DataSource = dtProdiLocal;
                    cmbProdi.DisplayMember = "NamaProdi";
                    cmbProdi.ValueMember = "NamaProdi";
                    if (cmbProdi.Items.Count > 0)
                        cmbProdi.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Tabel ProgramStudi kosong! Tambahkan data.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data prodi: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            // ============================================
            // 3. AKTIFKAN TOMBOL LOAD & CETAK
            // ============================================
            btnLoad.Enabled = true;
            btnLoad.Click += new EventHandler(btnLoad_Click);

            btnCetak.Enabled = true;
            btnCetak.Click += new EventHandler(btnCetak_Click);
        }

        private void RekapMahasiswa_Load(object sender, EventArgs e) { }

        // ============================================
        // TOMBOL LOAD
        // ============================================
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (cmbProdi.SelectedValue == null || string.IsNullOrEmpty(cmbProdi.SelectedValue.ToString()))
            {
                MessageBox.Show("Pilih Prodi terlebih dahulu!");
                return;
            }

            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@inProdi", SqlDbType.VarChar, 50).Value = cmbProdi.SelectedValue.ToString();
                cmd.Parameters.Add("@inTglMasuk", SqlDbType.Char, 4).Value = dtpTanggalMasuk.Value.Year.ToString();

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                dataGridView1.DataSource = dtMahasiswa;

                if (dtMahasiswa.Rows.Count > 0)
                {
                    MessageBox.Show("Data ditemukan: " + dtMahasiswa.Rows.Count + " baris.");
                }
                else
                {
                    MessageBox.Show("Data tidak ditemukan untuk filter tersebut.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        // ============================================
        // TOMBOL CETAK
        // ============================================
        private void btnCetak_Click(object sender, EventArgs e)
        {
            try
            {
                string prodiTerpilih = cmbProdi.SelectedValue != null ? cmbProdi.SelectedValue.ToString() : "";
                DateTime tanggalTerpilih = dtpTanggalMasuk.Value;

                Form3 frm3 = new Form3(prodiTerpilih, tanggalTerpilih);
                frm3.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuka halaman cetak: " + ex.Message);
            }
        }

        // Event lain (kosong)
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void cmbProdi_SelectedIndexChanged(object sender, EventArgs e) { }
        private void RekapMahasiswa_Load_1(object sender, EventArgs e) { } // Hapus method ini jika tidak dipakai
    }
}