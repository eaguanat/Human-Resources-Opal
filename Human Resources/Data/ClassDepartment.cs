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
    public class ClassDepartment
    {
        // Propiedades
        public int Id { get; set; }
        public string Description { get; set; }
        public int? Supervision { get; set; }
        public string ContractTemplateName { get; set; } // ¡NUEVA PROPIEDAD!

        // 1. LISTAR
        public DataTable Listar()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Consulta actualizada para incluir ContractTemplateName
                    string query = "SELECT Id, Description, Supervision, ContractTemplateName FROM tblDepartment ORDER BY Description";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving departments: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Consulta actualizada para incluir ContractTemplateName
                    string query = "INSERT INTO tblDepartment (Description, Supervision, ContractTemplateName) VALUES (@desc, @supervision, @template)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SetParameters(cmd);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting department: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Consulta actualizada para incluir ContractTemplateName
                    string query = "UPDATE tblDepartment SET Description = @desc, Supervision = @supervision WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    SetParameters(cmd);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating department: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 4. ELIMINAR (Sin cambios)
        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "DELETE FROM tblDepartment WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting department: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 5. VALIDACIÓN DE INTEGRIDAD (Sin cambios)
        public bool EstaEnUso(int idValidar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"SELECT 
                        (SELECT COUNT(*) FROM tblStaff WHERE idDepartment = @id) +
                        (SELECT COUNT(*) FROM tblApplicants WHERE idDepartment = @id)";

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
                return true;
            }
        }

        // Nuevo método para actualizar SOLO el nombre del archivo del contrato
        public bool ActualizarSoloPlantilla(int idDepto, string nombreArchivo)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Solo modificamos la columna del contrato filtrando por el ID
                    string query = "UPDATE tblDepartment SET ContractTemplateName = @template WHERE Id = @id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idDepto);
                    // Si el nombre viene vacío, mandamos DBNull a la base de datos
                    cmd.Parameters.AddWithValue("@template", (object)nombreArchivo ?? DBNull.Value);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating template name: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Método Helper actualizado para incluir el nombre de la plantilla.
        /// </summary>
        private void SetParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@desc", (object)this.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@supervision", (object)this.Supervision ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@template", (object)this.ContractTemplateName ?? DBNull.Value);
        }
    }
}