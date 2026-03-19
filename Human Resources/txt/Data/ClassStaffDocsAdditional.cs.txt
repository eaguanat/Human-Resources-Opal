using System;
using System.Data;
using System.Data.SqlClient;
using Human_Resources.Data;

namespace Human_Resources.Data
{
    public class ClassStaffDocsAdditional
    {
        public int IdStaff { get; set; }
        public int IdDocsAdditional { get; set; }
        public DateTime? DateDoc { get; set; }

        // Listar Staff para el DataGrid inicial
        public DataTable ListarStaff(string filtro, int idDept)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = @"SELECT Id, Name, LastName FROM tblStaff 
                             WHERE idDepartment = @idDept 
                             AND (Name LIKE @f OR LastName LIKE @f)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@idDept", idDept);
                cmd.Parameters.AddWithValue("@f", "%" + filtro + "%");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }


        // Trae TODOS los docs disponibles y une las fechas si el Staff ya las tiene
        public DataTable ListarDocumentosPorStaff(int idStaff)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT D.Id, D.Required, D.Description, S.DateDoc 
                             FROM tblDocsAdditional D
                             LEFT JOIN tblStaffDocsAdditional S ON D.Id = S.idDocsAdditional AND S.idStaff = @idStaff
                             WHERE D.Required = 1    
                             ORDER BY D.Required DESC, D.Description ASC";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@idStaff", idStaff);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public bool GuardarMasivo(int idStaff, DataTable dtDocumentos)
        {
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                con.Open();
                SqlTransaction tra = con.BeginTransaction();
                try
                {
                    // 1. Borrar registros previos
                    string delQuery = "DELETE FROM tblStaffDocsAdditional WHERE idStaff = @idStaff";
                    SqlCommand delCmd = new SqlCommand(delQuery, con, tra);
                    delCmd.Parameters.AddWithValue("@idStaff", idStaff);
                    delCmd.ExecuteNonQuery();

                    // 2. Insertar solo los que tienen fecha
                    string insQuery = "INSERT INTO tblStaffDocsAdditional (idStaff, idDocsAdditional, DateDoc) VALUES (@idStaff, @idDoc, @fecha)";
                    foreach (DataRow row in dtDocumentos.Rows)
                    {
                        if (row["DateDoc"] != DBNull.Value)
                        {
                            SqlCommand insCmd = new SqlCommand(insQuery, con, tra);
                            insCmd.Parameters.AddWithValue("@idStaff", idStaff);
                            insCmd.Parameters.AddWithValue("@idDoc", row["Id"]);
                            insCmd.Parameters.AddWithValue("@fecha", row["DateDoc"]);
                            insCmd.ExecuteNonQuery();
                        }
                    }
                    tra.Commit();
                    return true;
                }
                catch
                {
                    tra.Rollback();
                    return false;
                }
            }
        }

    }
}