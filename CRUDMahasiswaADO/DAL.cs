using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    internal class DAL
    {
        private SqlConnection conn = new SqlConnection(GetConnectionString());
        private SqlDataAdapter da;
        private DataTable dtMahasiswa;

        public static string GetConnectionString()
        {
            // Gunakan connection string statis yang sudah terbukti berhasil
            return "Data Source=LAPTOP-M4FI536S\\RAMDANIAA;Initial Catalog=DBAkademikADO;Integrated Security=True;";
        }

        // ==================== COUNT ====================
        public int CountMhs()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
            outputParam.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();
            conn.Close();
            return Convert.ToInt32(outputParam.Value);
        }

        // ==================== GET ALL ====================
        
    }
}