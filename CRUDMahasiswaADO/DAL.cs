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
        public DataTable GetMhs()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            da = new SqlDataAdapter(cmd);
            dtMahasiswa = new DataTable();
            da.Fill(dtMahasiswa);
            conn.Close();
            return dtMahasiswa;
        }

        // ==================== INSERT ====================
        public void InsertMhs(string nim, string nama, string alamat, string jenisKelamin,
                               DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand command = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("pNIM", nim);
                command.Parameters.AddWithValue("pNama", nama);
                command.Parameters.AddWithValue("pAlamat", alamat);
                command.Parameters.AddWithValue("pTanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("pJenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("pKodeProdi", kodeProdi);

                SqlParameter fotoParam = new SqlParameter("pFoto", SqlDbType.VarBinary);
                fotoParam.Value = (foto == null) ? (object)DBNull.Value : foto;
                command.Parameters.Add(fotoParam);

                command.ExecuteNonQuery();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        // ==================== UPDATE ====================
        public void UpdateMhs(string nim, string nama, string alamat, string jenisKelamin,
                               DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand command = new SqlCommand("sp_UpdateMahasiswa", conn, trans);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("pNIM", nim);
                command.Parameters.AddWithValue("pNama", nama);
                command.Parameters.AddWithValue("pAlamat", alamat);
                command.Parameters.AddWithValue("pTanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("pJenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("pKodeProdi", kodeProdi);

                SqlParameter fotoParam = new SqlParameter("pFoto", SqlDbType.VarBinary);
                fotoParam.Value = (foto == null || foto.Length == 0) ? (object)DBNull.Value : foto;
                command.Parameters.Add(fotoParam);

                command.ExecuteNonQuery();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        // ==================== DELETE ====================
        public void DeleteMhs(string nim)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@NIM", nim);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // ==================== GET FOTO BY NIM (UNTUK MEMPERTAHANKAN FOTO SAAT UPDATE) ====================
        public byte[] GetFotoByNIM(string nim)
        {
            byte[] foto = null;
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT foto FROM Mahasiswa WHERE NIM = @NIM", conn);
                cmd.Parameters.AddWithValue("@NIM", nim);
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                    foto = (byte[])result;
                conn.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetFotoByNIM error: " + ex.Message);
            }
            return foto;
        }

        // ==================== RESET DATA ====================
        
    }
}