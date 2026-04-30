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
        public int? Supervision { get; set; } // ¡NUEVA PROPIEDAD! (int? para permitir nulls)

        // 1. LISTAR
        public DataTable Listar()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // ¡Consulta actualizada para incluir Supervision!
                    string query = "SELECT Id, Description, Supervision FROM tblDepartment ORDER BY Description";
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
                    // ¡Consulta actualizada para incluir Supervision!
                    string query = "INSERT INTO tblDepartment (Description, Supervision) VALUES (@desc, @supervision)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SetParameters(cmd); // Usamos el nuevo método SetParameters
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
                    // ¡Consulta actualizada para incluir Supervision!
                    string query = "UPDATE tblDepartment SET Description = @desc, Supervision = @supervision WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", this.Id); // El ID es necesario para el WHERE
                    SetParameters(cmd); // Usamos el nuevo método SetParameters
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

        // 4. ELIMINAR (No necesita cambios)
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

        // 5. VALIDACIÓN DE INTEGRIDAD (No necesita cambios, pero revisa si otros módulos usan Department)
        public bool EstaEnUso(int idValidar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Verificamos si esta en uso tablas de Staff y tblApplicants
                    string query = @"SELECT 
                        (SELECT COUNT(*) FROM tblStaff WHERE idDepartment = @id) +
                        (SELECT COUNT(*) FROM tblApplicants WHERE idDepartment = @id)"; // Añadido tblApplicants

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

        /// <summary>
        /// Método Helper para establecer los parámetros comunes.
        /// </summary>
        private void SetParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@desc", (object)this.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@supervision", (object)this.Supervision ?? DBNull.Value);
        }
    }
}