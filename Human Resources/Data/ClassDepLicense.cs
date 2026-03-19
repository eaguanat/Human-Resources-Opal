using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Human_Resources.Data
{
    public class ClassDepLicense
    {
        // Propiedades estándar
        public int Id { get; set; }
        public int IdDepartment { get; set; }
        public string Description { get; set; }

        // 1. LISTAR ciudades filtradas (Para el DataGrid de la pantalla)
        public DataTable Listar(int idFind)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Agregamos el nombre del estado con un JOIN para que el DataGrid también pueda mostrarlo si quieres
                    string query = @"SELECT C.Id, C.Description, S.Description as DptoDescription 
                                   FROM tblDepLicense C 
                                   INNER JOIN tblDepartment S ON C.IdDepartment = S.Id 
                                   WHERE C.idDepartment = @id 
                                   ORDER BY C.Description";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@id", idFind);
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
                    string query = @"SELECT C.Description, S.Description as tmpDescription 
                                   FROM tblDepLicense C 
                                   INNER JOIN tblDepartment S ON C.IdDepartment = S.Id 
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

        // --- Los métodos Insertar, Actualizar, Eliminar y EstaEnUso se mantienen igual ---
        // (Solo asegúrate de que sigan ahí al pegar)

        public bool Insertar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "INSERT INTO tblDepLicense (IdDepartment, Description) VALUES (@idFind, @desc)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@idFind", this.IdDepartment);
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
                    string query = "UPDATE tblDepLicense SET Description = @desc WHERE Id = @id";
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
                    string query = "DELETE FROM tblDepLicense WHERE Id = @id";
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
                (SELECT COUNT(*) FROM tblStaffLicense WHERE idDepLicense = @id)";

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