using System;
using System.Data;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form3 : Form
    {
        // Cukup deklarasikan ini saja:
        private CrystalReport1 listMahasiswa = new CrystalReport1();
        private DAL dbLogic = new DAL();

        private string prodi;
        private DateTime tglmasuk;

        public Form3(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();
            prodi = Prodi;
            tglmasuk = TglMasuk;

            try
            {
                // Ambil data dari DAL (semua logic SQL sudah di DAL)
                DataTable dtMahasiswa = dbLogic.getDataRekap(prodi, tglmasuk);

                listMahasiswa.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = listMahasiswa;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                // dbLogic.InsertLog(ex.Message); // Aktifkan jika perlu logging
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void Form3_Load(object sender, EventArgs e) { }
        private void crystalReportViewer2_Load(object sender, EventArgs e) { }
    }
}