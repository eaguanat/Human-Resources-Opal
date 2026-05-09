using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace Human_Resources.Data
{
    public class ClassCompany
    {
        // --- PROPIEDADES ---
        public int Id { get; set; }
        public string Name { get; set; }
        public int? IdGeoRegion { get; set; }
        public string Address { get; set; }
        public int? IdGeoState { get; set; }
        public int? IdGeoCity { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public byte[] Logo { get; set; }

        // Propiedades para la configuración de correo electrónico
        public string MailServer { get; set; }
        public int? MailPort { get; set; }
        public string MailUsername { get; set; }
        public string MailPasswordText { get; set; }
        public string MailPasswordHash { get; set; }
        public bool? EnableSSL { get; set; }
        public string SenderName { get; set; }

        // ¡NUEVAS PROPIEDADES PARA RUTAS DE CONTRATOS!
        public string PathTemplates { get; set; }
        public string PathContracts { get; set; }

        // --- MÉTODOS ---

        public bool Obtener()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT * FROM tblCompany WHERE id = 1";
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

                        this.MailServer = dr["MailServer"].ToString();
                        this.MailPort = dr["MailPort"] != DBNull.Value ? Convert.ToInt32(dr["MailPort"]) : (int?)null;
                        this.MailUsername = dr["MailUsername"].ToString();
                        this.MailPasswordHash = dr["MailPassword"].ToString();
                        this.EnableSSL = dr["EnableSSL"] != DBNull.Value ? Convert.ToBoolean(dr["EnableSSL"]) : (bool?)null;
                        this.SenderName = dr["SenderName"].ToString();

                        // Cargar las nuevas rutas
                        this.PathTemplates = dr["PathTemplates"].ToString();
                        this.PathContracts = dr["PathContracts"].ToString();
                        return true;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading company details: " + ex.Message, "ObtenerCompany"); }
            return false;
        }

        public bool Guardar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    con.Open();

                    if (!string.IsNullOrEmpty(this.MailPasswordText))
                    {
                        this.MailPasswordHash = ClassCfgUsers.HashPassword(this.MailPasswordText);
                    }
                    else if (string.IsNullOrEmpty(this.MailPasswordHash))
                    {
                        this.MailPasswordHash = null;
                    }

                    string query;
                    SqlCommand cmd;

                    if (Existe())
                    {
                        // Query actualizado con las nuevas columnas
                        query = @"UPDATE tblCompany SET 
                                    Name=@Name, Address=@Address, IdGeoRegion=@IdGeoRegion, 
                                    idGeoState=@idGeoState, idGeoCity=@idGeoCity, ZipCode=@ZipCode, Phone=@Phone, 
                                    Email=@Email, Logo=@Logo,
                                    MailServer=@MailServer, MailPort=@MailPort, MailUsername=@MailUsername, 
                                    MailPassword=@MailPassword, EnableSSL=@EnableSSL, SenderName=@SenderName,
                                    PathTemplates=@PathTemplates, PathContracts=@PathContracts
                                WHERE id=1";
                    }
                    else
                    {
                        query = @"INSERT INTO tblCompany (Id, Name, Address, IdGeoRegion, idGeoState, idGeoCity, ZipCode, Phone, Email, Logo, 
                                MailServer, MailPort, MailUsername, MailPassword, EnableSSL, SenderName, PathTemplates, PathContracts) 
                                VALUES (1, @Name, @Address, @IdGeoRegion, @idGeoState, @idGeoCity, @ZipCode, @Phone, @Email, @Logo, 
                                @MailServer, @MailPort, @MailUsername, @MailPassword, @EnableSSL, @SenderName, @PathTemplates, @PathContracts);";
                    }

                    cmd = new SqlCommand(query, con);
                    SetParameters(cmd);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Save Company): " + ex.Message, "GuardarCompany"); return false; }
        }

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

            cmd.Parameters.AddWithValue("@MailServer", (object)MailServer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MailPort", (object)MailPort ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MailUsername", (object)MailUsername ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MailPassword", (object)MailPasswordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EnableSSL", (object)EnableSSL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SenderName", (object)SenderName ?? DBNull.Value);

            // Parámetros para las nuevas rutas
            cmd.Parameters.AddWithValue("@PathTemplates", (object)PathTemplates ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PathContracts", (object)PathContracts ?? DBNull.Value);
        }

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
    }
}