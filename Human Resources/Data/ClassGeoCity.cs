using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Human_Resources.Data
{
    public class ClassGeoCity
    {
        // Propiedades estándar
        public int Id { get; set; }
        public int IdGeoState { get; set; }
        public string Description { get; set; }

        // 1. LISTAR ciudades filtradas (Para el DataGrid de la pantalla)
        public DataTable ListarPorEstado(int idState)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Agregamos el nombre del estado con un JOIN para que el DataGrid también pueda mostrarlo si quieres
                    string query = @"SELECT C.Id, C.Description, S.Description as StateDescription 
                                   FROM tblGeoCity C 
                                   INNER JOIN tblGeoState S ON C.IdGeoState = S.Id 
                                   WHERE C.IdGeoState = @id 
                                   ORDER BY C.Description";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@id", idState);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error database (List): " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dt;
        }

        // 6. NUEVO: LISTAR TODO PARA REPORTE (Agrupado por Estado)
        // Este es el método que usaremos en el BtnPrint_Click
        public DataTable ListarTodoParaReporte()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"SELECT C.Description, S.Description as StateDescription 
                                   FROM tblGeoCity C 
                                   INNER JOIN tblGeoState S ON C.IdGeoState = S.Id 
                                   ORDER BY S.Description, C.Description";

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
                    string query = "INSERT INTO tblGeoCity (IdGeoState, Description) VALUES (@idState, @desc)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@idState", this.IdGeoState);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
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
                    string query = "UPDATE tblGeoCity SET Description = @desc WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@desc", this.Description);
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
                    string query = "DELETE FROM tblGeoCity WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error database (Delete): " + ex.Message); return false; }
        }

        // 5. VALIDAR INTEGRIDAD (Para evitar borrar ciudades que ya tienen personal asignado)
        public bool EstaEnUso(int idValidar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Sumamos el conteo de ambas tablas de Staff (Nursing y LTC)
                    string query = @"SELECT 
                (SELECT COUNT(*) FROM tblStaff WHERE idGeoCity = @id)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idValidar);
                    con.Open();

                    int total = Convert.ToInt32(cmd.ExecuteScalar());
                    return total > 0; // Si es mayor a 0, la ciudad está siendo usada.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error database (Validation): " + ex.Message);
                return true; // Por seguridad, si falla la consulta, asumimos que está en uso para no borrar
            }
        }
    }
}