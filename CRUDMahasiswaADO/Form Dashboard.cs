using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CRUDMahasiswaADO
{
    public partial class FormDashboard : Form
    {
        DAL dbLogic = new DAL();
        bool isInitializing = true;
        DataTable dt;
        int button = 0;

        public FormDashboard()
        {
            InitializeComponent();

            // ============================================
            // PASANG SEMUA EVENT HANDLER SECARA MANUAL
            // (agar pasti terhubung walau di designer belum)
            // ============================================
            this.Load += FormDashboard_Load;
            cmbTipe.SelectedValueChanged += cmbTipe_SelectedValueChanged;
            btnLoad.Click += btnLoad_Click;
            btnReset.Click += btnReset_Click;

        }

        private void FormDashboard_Load(object sender, EventArgs e)
        {
            // Setup DateTimePicker
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            // Setup ComboBox Tipe Chart
            cmbTipe.DropDownStyle = ComboBoxStyle.DropDownList;
            var items = new List<KeyValuePair<string, SeriesChartType>>
            {
                new KeyValuePair<string, SeriesChartType>("Column", SeriesChartType.Column),
                new KeyValuePair<string, SeriesChartType>("Pie", SeriesChartType.Pie)
            };

            isInitializing = true;
            cmbTipe.DataSource = items;
            cmbTipe.DisplayMember = "Key";
            cmbTipe.ValueMember = "Value";
            cmbTipe.SelectedIndex = 0;

            // Selesai setup, izinkan event combo untuk berjalan
            isInitializing = false;

            // Load chart pertama kali (menampilkan semua data)
            button = 0;
            loadDataChart();
        }

        // Event ketika combo box berubah (Column/Pie)
        private void cmbTipe_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isInitializing) return;
            loadDataChart(); // Refresh chart sesuai tipe yang dipilih
        }

        public void loadDataChart()
        {
            chartProdi.Series.Clear();
            chartProdi.Titles.Clear();
            chartProdi.Legends.Clear();
            chartProdi.ChartAreas.Clear();

            // Setup Area Chart
            ChartArea ca = new ChartArea("MainArea");
            ca.AxisX.Title = "Program Studi";
            ca.AxisY.Title = "Jumlah Mahasiswa";
            ca.AxisX.LabelStyle.Angle = -45;
            ca.BackColor = Color.Transparent;
            chartProdi.ChartAreas.Add(ca);

            try
            {
                // Ambil data berdasarkan tombol (Load atau Reset)
                if (button == 1)
                    dt = dbLogic.getDataChartByTahun(dtpTanggalMasuk.Value);
                else
                    dt = dbLogic.getAllDataChart();

                // Cek apakah data kosong
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data mahasiswa untuk ditampilkan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SeriesChartType tipe = (SeriesChartType)cmbTipe.SelectedValue;

                if (tipe == SeriesChartType.Column)
                {
                    Series s = new Series("Mahasiswa");
                    s.ChartType = SeriesChartType.Column;
                    foreach (DataRow row in dt.Rows)
                    {
                        string prodi = row["NamaProdi"].ToString();
                        int jumlah = Convert.ToInt32(row["JumlahMhs"]);
                        s.Points.AddXY(prodi, jumlah);
                    }
                    chartProdi.Series.Add(s);
                }
                else // Pie
                {
                    Series s = new Series("Jumlah Mahasiswa");
                    s.ChartType = tipe;
                    s.IsValueShownAsLabel = true;
                    s.Label = "#VAL";
                    s.LegendText = "#VALX";
                    foreach (DataRow row in dt.Rows)
                    {
                        string prodi = row["NamaProdi"].ToString();
                        int jumlah = Convert.ToInt32(row["JumlahMhs"]);
                        s.Points.AddXY(prodi, jumlah);
                    }
                    chartProdi.Series.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load chart: " + ex.Message);
            }

            // Title & Legend
            Title title = new Title("Jumlah Mahasiswa per Program Studi", Docking.Top, new Font("Arial", 14, FontStyle.Bold), Color.DarkBlue);
            chartProdi.Titles.Add(title);
            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Right;
            chartProdi.Legends.Add(legend);
        }

        // Event Tombol Load (Filter berdasarkan Tahun)
        private void btnLoad_Click(object sender, EventArgs e)
        {
            button = 1;
            loadDataChart();
        }

        // Event Tombol Reset (Semua Tahun)
        private void btnReset_Click(object sender, EventArgs e)
        {
            button = 0;
            loadDataChart();
        }

        // Event Tombol ke Form Data Mahasiswa
        private void btnDatamahasiswa_Click(object sender, EventArgs e)
        {
            Form1 frm1 = new Form1();
            frm1.Show();
            this.Hide();
        }
    }
}