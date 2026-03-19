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
    public class ClassDocsAdditional
    {
        // Propiedades
        public int Id { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }


        // 1. LISTAR
        public DataTable Listar()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT Id, Description, Required FROM tblDocsAdditional ORDER BY Description";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving regions: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dt;
        }

        // 2. INSERTAR
        public bool Insertar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "INSERT INTO tblDocsAdditional (Description, Required) VALUES (@desc, @required)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
                    cmd.Parameters.AddWithValue("@required", this.Required);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting : " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 3. ACTUALIZAR
        public bool Actualizar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "UPDATE tblDocsAdditional SET Description = @desc, Required = @required WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
                    cmd.Parameters.AddWithValue("@required", this.Required);
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating : " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 4. ELIMINAR
        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "DELETE FROM tblDocsAdditional WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting : " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 5. VALIDACIÓN DE INTEGRIDAD (NUEVO)
        public bool EstaEnUso(int idValidar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Verificamos si esta en uso tablas de Staff
                    string query = "SELECT COUNT(*) FROM tblStaffDocsAdditional WHERE idDocsAdditional = @id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idValidar);
                    con.Open();

                    int total = (int)cmd.ExecuteScalar();
                    return total > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Integrity check error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true; // Por seguridad bloqueamos el borrado si falla la consulta
            }
        }
    }
}