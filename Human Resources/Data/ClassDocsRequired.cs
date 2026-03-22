using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Human_Resources.Data
{
    public class ClassDocsRequired
    {
        // Propiedades estándar
        public int Id { get; set; }
        public int IdDepartment { get; set; }
        public int IdSection { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }


        // 1. LISTAR (Filtrado por Departamento para el DataGrid)
        public DataTable Listar(int idFindA, int idFindB)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // S.Description es el nombre del departamento
                    string query = @"SELECT C.Id, C.IdSection, C.Description, C.Required, S.Description as DepartmentName 
                                   FROM tblDocsRequired C 
                                   INNER JOIN tblDepartment S ON C.idDepartment = S.id 
                                   WHERE C.idDepartment = @id and C.IdSection = @idSection
                                   ORDER BY C.Description ASC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@id", idFindA);
                    da.SelectCommand.Parameters.AddWithValue("@idSection", idFindB);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error database (List): " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dt;
        }

        // 2. LISTAR TODO PARA REPORTE
        public DataTable ListarTodoParaReporte()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Ordenamos por Departamento, luego Inservice (True primero) y luego Alfabético
                    string query = @"SELECT S.Description as DepartmentName, C.Description , C.idSection, C.Required
                                   FROM tblDocsRequired C 
                                   INNER JOIN tblDepartment S ON C.idDepartment = S.id 
                                   ORDER BY S.Description ASC, C.IdSection ASC, C.Description ASC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error database (Report): " + ex.Message);
            }
            return dt;
        }

        public bool Insertar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Corregido: idDepartment e Inservice
                    string query = "INSERT INTO tblDocsRequired (idDepartment, idSection, Description, Required) VALUES (@idDepartment, @idSection, @desc, @Required )";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@idDepartment", this.IdDepartment);
                    cmd.Parameters.AddWithValue("@idSection", this.IdSection);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
                    cmd.Parameters.AddWithValue("@Required", this.Required);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error database (Insert): " + ex.Message); return false; }
        }

        public bool Actualizar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "UPDATE tblDocsRequired SET idDepartment = @idDepartment, idSection = @idSection, Description = @desc, Required = @Required WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@idDepartment", this.IdDepartment);
                    cmd.Parameters.AddWithValue("@idSection", this.IdSection);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
                    cmd.Parameters.AddWithValue("@Required", this.Required);
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error database (Update): " + ex.Message); return false; }
        }

        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "DELETE FROM tblDocsRequired WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error database (Delete): " + ex.Message); return false; }
        }

        // 5. VALIDAR INTEGRIDAD (Usa la nueva tabla tblStaffDocsRequired)
        public bool EstaEnUso(int idValidar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Verificamos si este documento ya fue asignado a algún Staff
                    string query = "SELECT COUNT(*) FROM tblStaffDocsRequired WHERE idDocsRequired = @id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idValidar);
                    con.Open();

                    int total = Convert.ToInt32(cmd.ExecuteScalar());
                    return total > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error database (Validation): " + ex.Message);
                return true;
            }
        }
    }
}