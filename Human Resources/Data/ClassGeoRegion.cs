using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Human_Resources.Data
{
    public class ClassGeoRegion
    {
        // Propiedades
        public int Id { get; set; }
        public string Description { get; set; }

        // 1. LISTAR
        public DataTable Listar()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT Id, Description FROM tblGeoRegion ORDER BY Description";
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
                    string query = "INSERT INTO tblGeoRegion (Description) VALUES (@desc)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting region: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    string query = "UPDATE tblGeoRegion SET Description = @desc WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating region: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    string query = "DELETE FROM tblGeoRegion WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting region: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Verificamos si la región existe en las tablas de Staff
                    string query = @"SELECT 
                        (SELECT COUNT(*) FROM tblStaff WHERE idGeoRegion = @id)";

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