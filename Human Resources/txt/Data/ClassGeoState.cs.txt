using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; // Necesario para el MessageBox

namespace Human_Resources.Data
{
    public class ClassGeoState
    {
        // --- PROPERTIES (Get and Set) ---
        public int Id { get; set; }
        public string Description { get; set; }

        // --- OPERATION METHODS ---

        // 1. Method to INSERT
        public bool Insertar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "INSERT INTO tblGeoState (Description) VALUES (@desc)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@desc", this.Description);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while inserting: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 2. Method to LIST (Load DataGrid)
        public DataTable Listar()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT Id, Description FROM tblGeoState ORDER BY Description";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while retrieving data: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // 3. Method to UPDATE
        public bool Actualizar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "UPDATE tblGeoState SET Description = @desc WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
                    cmd.Parameters.AddWithValue("@id", this.Id);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 4. Method to DELETE
        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "DELETE FROM tblGeoState WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // 5. Method to VALIDATE RELATIONSHIPS
        public bool EstaEnUso(int idValidar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"SELECT 
                        (SELECT COUNT(*) FROM tblStaff WHERE idGeoState = @id) + 
                        (SELECT COUNT(*) FROM tblGeoCity WHERE idGeoState = @id)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idValidar);

                    con.Open();
                    int totalRecords = (int)cmd.ExecuteScalar();

                    return totalRecords > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while validating data integrity: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }
    }
}
