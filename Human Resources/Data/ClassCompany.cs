using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows; // Para MessageBox, aunque idealmente en una capa de datos no se debería.

namespace Human_Resources.Data
{
    public class ClassCompany
    {
        // --- PROPIEDADES ---
        public int Id { get; set; } // Aunque siempre será 1, lo mantenemos por consistencia
        public string Name { get; set; }
        public int? IdGeoRegion { get; set; }
        public string Address { get; set; }
        public int? IdGeoState { get; set; }
        public int? IdGeoCity { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public byte[] Logo { get; set; }

        // Propiedades para la configuración de correo electrónico (NUEVAS)
        public string MailServer { get; set; }
        public int? MailPort { get; set; }
        public string MailUsername { get; set; }
        public string MailPasswordText { get; set; } // Contraseña en texto plano para el UI/hashing
        public string MailPasswordHash { get; set; } // Contraseña hasheada para la BD
        public bool? EnableSSL { get; set; }
        public string SenderName { get; set; }

        // --- MÉTODOS ---

        /// <summary>
        /// Obtiene los datos de la compañía (siempre ID = 1).
        /// </summary>
        public bool Obtener()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT * FROM tblCompany WHERE id = 1"; // Siempre buscamos el ID 1
                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        this.Id = Convert.ToInt32(dr["id"]);
                        this.Name = dr["Name"].ToString();
                        this.Address = dr["Address"].ToString();
                        this.IdGeoRegion = dr["IdGeoRegion"] != DBNull.Value ? Convert.ToInt32(dr["IdGeoRegion"]) : (int?)null;
                        this.IdGeoState = dr["idGeoState"] != DBNull.Value ? Convert.ToInt32(dr["idGeoState"]) : (int?)null;
                        this.IdGeoCity = dr["idGeoCity"] != DBNull.Value ? Convert.ToInt32(dr["idGeoCity"]) : (int?)null;
                        this.ZipCode = dr["ZipCode"].ToString();
                        this.Phone = dr["Phone"].ToString();
                        this.Email = dr["Email"].ToString();
                        this.Logo = dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : null;

                        // Cargar propiedades de correo
                        this.MailServer = dr["MailServer"].ToString();
                        this.MailPort = dr["MailPort"] != DBNull.Value ? Convert.ToInt32(dr["MailPort"]) : (int?)null;
                        this.MailUsername = dr["MailUsername"].ToString();
                        this.MailPasswordHash = dr["MailPassword"].ToString(); // Cargar el hash para futuras verificaciones
                        this.EnableSSL = dr["EnableSSL"] != DBNull.Value ? Convert.ToBoolean(dr["EnableSSL"]) : (bool?)null;
                        this.SenderName = dr["SenderName"].ToString();
                        return true;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading company details: " + ex.Message, "ObtenerCompany"); }
            return false;
        }

        /// <summary>
        /// Verifica si ya existe un registro con ID = 1.
        /// </summary>
        public bool Existe()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT COUNT(*) FROM tblCompany WHERE id = 1";
                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking if company exists: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Inserta o actualiza la configuración de la compañía (siempre ID = 1).
        /// </summary>
        public bool Guardar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    con.Open();

                    // --- Manejo de la Contraseña de Correo ---
                    if (!string.IsNullOrEmpty(this.MailPasswordText)) // Si se ha introducido una nueva contraseña
                    {
                        this.MailPasswordHash = ClassCfgUsers.HashPassword(this.MailPasswordText);
                    }
                    else if (string.IsNullOrEmpty(this.MailPasswordHash)) // Si no se ha introducido nueva y no hay hash previo, asegurar NULL o vacío.
                    {
                        this.MailPasswordHash = null;
                    }
                    // Si MailPasswordText está vacío pero MailPasswordHash ya tiene un valor (de una carga previa),
                    // no lo modificamos, manteniendo la contraseña existente.


                    string query;
                    SqlCommand cmd;

                    if (Existe()) // Si ya existe el registro con ID=1, actualizamos
                    {
                        query = @"UPDATE tblCompany SET 
                                    Name=@Name, Address=@Address, IdGeoRegion=@IdGeoRegion, 
                                    idGeoState=@idGeoState, idGeoCity=@idGeoCity, ZipCode=@ZipCode, Phone=@Phone, 
                                    Email=@Email, Logo=@Logo,
                                    MailServer=@MailServer, MailPort=@MailPort, MailUsername=@MailUsername, 
                                    MailPassword=@MailPassword, EnableSSL=@EnableSSL, SenderName=@SenderName
                                WHERE id=1";
                        cmd = new SqlCommand(query, con);
                    }
                    else // Si no existe, insertamos con ID=1 (si la columna Id es Identity, SQL Server la ignorará, pero intentaremos forzar el 1)
                    {
                        // Nota: Si Id es IDENTITY, no podemos insertar el ID manualmente.
                        // La estrategia es insertar y luego actualizar para asegurarnos de que el ID sea 1.
                        // Sin embargo, lo más seguro es usar el Update si ya existe o Insertar uno nuevo si no.
                        // Para esta lógica de "único registro", la forma más sencilla es siempre hacer UPDATE
                        // si Existe() devuelve true. Si no existe, se inserta una vez.
                        query = @"INSERT INTO tblCompany (Id, Name, Address, IdGeoRegion, idGeoState, idGeoCity, ZipCode, Phone, Email, Logo, 
                                MailServer, MailPort, MailUsername, MailPassword, EnableSSL, SenderName) 
                                VALUES (1, @Name, @Address, @IdGeoRegion, @idGeoState, @idGeoCity, @ZipCode, @Phone, @Email, @Logo, 
                                @MailServer, @MailPort, @MailUsername, @MailPassword, @EnableSSL, @SenderName);";
                        cmd = new SqlCommand(query, con);
                    }

                    SetParameters(cmd);
                    // Si se inserta un nuevo registro, el SCOPE_IDENTITY() devolvería el ID autogenerado,
                    // pero para un registro singleton, realmente no nos interesa.
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Save Company): " + ex.Message, "GuardarCompany"); return false; }
        }

        /// <summary>
        /// Helper para establecer TODOS los parámetros para INSERT/UPDATE.
        /// </summary>
        private void SetParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Name", (object)Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object)Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdGeoRegion", (object)IdGeoRegion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoState", (object)IdGeoState ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoCity", (object)IdGeoCity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ZipCode", (object)ZipCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);

            SqlParameter paramLogo = new SqlParameter("@Logo", SqlDbType.VarBinary);
            paramLogo.Value = (object)Logo ?? DBNull.Value;
            cmd.Parameters.Add(paramLogo);

            // Parámetros de Correo Electrónico
            cmd.Parameters.AddWithValue("@MailServer", (object)MailServer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MailPort", (object)MailPort ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MailUsername", (object)MailUsername ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MailPassword", (object)MailPasswordHash ?? DBNull.Value); // Usamos el hash
            cmd.Parameters.AddWithValue("@EnableSSL", (object)EnableSSL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SenderName", (object)SenderName ?? DBNull.Value);
        }
    }
}