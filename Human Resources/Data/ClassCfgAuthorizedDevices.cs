using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; // Para MessageBox, aunque es mejor que las capas de datos no lo usen directamente.

namespace Human_Resources.Data
{
    public class ClassCfgAuthorizedDevices
    {
        // --- PROPIEDADES ---
        public int Id { get; set; }
        public string DeviceHash { get; set; }
        public string DeviceName { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }

        // --- MÉTODOS CRUD ---

        /// <summary>
        /// Lista todos los dispositivos autorizados o filtra por nombre.
        /// </summary>
        public DataTable Listar(string filtro = "")
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT Id, DeviceHash, DeviceName, IsActive, RegistrationDate FROM cfgAuthorizedDevices WHERE DeviceName LIKE @f OR DeviceHash LIKE @f ORDER BY DeviceName";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@f", "%" + filtro + "%");
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving authorized devices: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dt;
        }

        /// <summary>
        /// Inserta un nuevo dispositivo autorizado y devuelve su ID.
        /// </summary>
        public int Insertar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"INSERT INTO cfgAuthorizedDevices (DeviceHash, DeviceName, IsActive, RegistrationDate)
                                     VALUES (@DeviceHash, @DeviceName, @IsActive, @RegistrationDate);
                                     SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SetParameters(cmd);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (SqlException ex)
            {
                // Manejar error de clave única si el DeviceHash ya existe
                if (ex.Number == 2627) // Código de error para violaciones de clave única
                {
                    MessageBox.Show("This Device ID (Hash) is already registered.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Error inserting authorized device: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting authorized device: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        /// <summary>
        /// Actualiza un dispositivo autorizado existente.
        /// </summary>
        public bool Actualizar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"UPDATE cfgAuthorizedDevices SET DeviceHash=@DeviceHash, DeviceName=@DeviceName, IsActive=@IsActive, RegistrationDate=@RegistrationDate
                                     WHERE Id=@Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", this.Id);
                    SetParameters(cmd);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    MessageBox.Show("This Device ID (Hash) is already registered to another device.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Error updating authorized device: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating authorized device: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Elimina un dispositivo autorizado por ID.
        /// </summary>
        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "DELETE FROM cfgAuthorizedDevices WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting authorized device: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Obtiene los detalles de un dispositivo autorizado por su ID.
        /// </summary>
        public bool ObtenerPorId(int idBusqueda)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT Id, DeviceHash, DeviceName, IsActive, RegistrationDate FROM cfgAuthorizedDevices WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", idBusqueda);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        this.Id = Convert.ToInt32(dr["Id"]);
                        this.DeviceHash = dr["DeviceHash"].ToString();
                        this.DeviceName = dr["DeviceName"].ToString();
                        this.IsActive = Convert.ToBoolean(dr["IsActive"]);
                        this.RegistrationDate = Convert.ToDateTime(dr["RegistrationDate"]);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading authorized device details: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        /// <summary>
        /// Helper para establecer parámetros SQL.
        /// </summary>
        private void SetParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@DeviceHash", (object)DeviceHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DeviceName", (object)DeviceName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", IsActive); // bool se convierte automáticamente a bit en SQL
            cmd.Parameters.AddWithValue("@RegistrationDate", RegistrationDate);
        }
    }
}