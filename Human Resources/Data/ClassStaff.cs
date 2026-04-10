using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace Human_Resources.Data
{
    public class ClassStaff
    {
        // --- PROPIEDADES ---
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int? IdDepartment { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? AppDay { get; set; }
        public DateTime? HiredDay { get; set; }
        public DateTime? EndDay { get; set; }
        public string Address { get; set; }
        public int? IdGeoRegion { get; set; }
        public int? IdGeoState { get; set; }
        public int? IdGeoCity { get; set; }
        public string ZipCod { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ContactPersonA { get; set; }
        public string ContactPhoneA { get; set; }
        public string ContactPersonB { get; set; }
        public string ContactPhoneB { get; set; }
        public int? IdDocsPayroll { get; set; }
        public string CompanyName { get; set; }
        public int? IdBanks { get; set; }
        public int? IdBanksMethod { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public int? IdDocsInmigration { get; set; }
        public string Social { get; set; }
        public double Rate { get; set; }
        public string JobDes { get; set; }
        public bool Active { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // --- MÉTODOS ---

        public bool ObtenerPorId(int idBusqueda)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    string query = "SELECT * FROM tblStaff WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idBusqueda);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        this.Id = Convert.ToInt32(dr["id"]);
                        this.Name = dr["Name"].ToString();
                        this.LastName = dr["LastName"].ToString();
                        this.IdDepartment = dr["IdDepartment"] != DBNull.Value ? Convert.ToInt32(dr["IdDepartment"]) : (int?)null;
                        this.Birthday = dr["Birthday"] != DBNull.Value ? Convert.ToDateTime(dr["Birthday"]) : (DateTime?)null;
                        this.AppDay = dr["AppDay"] != DBNull.Value ? Convert.ToDateTime(dr["AppDay"]) : (DateTime?)null;
                        this.HiredDay = dr["HiredDay"] != DBNull.Value ? Convert.ToDateTime(dr["HiredDay"]) : (DateTime?)null;
                        this.EndDay = dr["EndDay"] != DBNull.Value ? Convert.ToDateTime(dr["EndDay"]) : (DateTime?)null;
                        this.Address = dr["Address"].ToString();
                        this.IdDocsInmigration = dr["IdDocsInmigration"] != DBNull.Value ? Convert.ToInt32(dr["IdDocsInmigration"]) : (int?)null;
                        this.IdGeoRegion = dr["IdGeoRegion"] != DBNull.Value ? Convert.ToInt32(dr["IdGeoRegion"]) : (int?)null;
                        this.IdGeoState = dr["idGeoState"] != DBNull.Value ? Convert.ToInt32(dr["idGeoState"]) : (int?)null;
                        this.IdGeoCity = dr["idGeoCity"] != DBNull.Value ? Convert.ToInt32(dr["idGeoCity"]) : (int?)null;
                        this.ZipCod = dr["ZipCod"].ToString();
                        this.Phone = dr["Phone"].ToString();
                        this.Email = dr["Email"].ToString();
                        this.ContactPersonA = dr["ContactPersonA"].ToString();
                        this.ContactPhoneA = dr["ContactPhoneA"].ToString();
                        this.ContactPersonB = dr["ContactPersonB"].ToString();
                        this.ContactPhoneB = dr["ContactPhoneB"].ToString();
                        this.IdDocsPayroll = dr["idDocsPayroll"] != DBNull.Value ? Convert.ToInt32(dr["idDocsPayroll"]) : (int?)null;
                        this.CompanyName = dr["CompanyName"].ToString();
                        this.IdBanks = dr["idBanks"] != DBNull.Value ? Convert.ToInt32(dr["idBanks"]) : (int?)null;
                        this.IdBanksMethod = dr["idBanksMethod"] != DBNull.Value ? Convert.ToInt32(dr["idBanksMethod"]) : (int?)null;
                        this.AccountNumber = dr["AccountNumber"].ToString();
                        this.RoutingNumber = dr["RoutingNumber"].ToString();
                        this.Social = dr["Social"].ToString();
                        this.Rate = (dr["Rate"] != DBNull.Value) ? Convert.ToDouble(dr["Rate"]) : 0.0;
                        this.JobDes = dr["JobDes"].ToString();
                        this.Active = dr["Active"] != DBNull.Value && Convert.ToBoolean(dr["Active"]);
                        this.Latitude = dr["Latitude"] != DBNull.Value ? Convert.ToDecimal(dr["Latitude"]) : (decimal?)null; //
                        this.Longitude = dr["Longitude"] != DBNull.Value ? Convert.ToDecimal(dr["Longitude"]) : (decimal?)null; //
                        return true;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading details: " + ex.Message, "ObtenerPorId"); }
            return false;
        }
        public DataTable Listar(string filtro = "")
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    string query = @"SELECT SN.id, SN.Name, SN.LastName, SN.idDepartment, SN.AppDay, SN.EndDay, SN.HiredDay, SN.JobDes, SN.Phone, SN.Social, SN.Active, 
                                            S.Description as StateName
                                     FROM tblStaff SN
                                     LEFT JOIN tblGeoState S ON SN.idGeoState = S.Id
                                     WHERE SN.Name LIKE @f OR SN.LastName LIKE @f
                                     ORDER BY SN.LastName, SN.Name";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@f", "%" + filtro + "%");
                    da.Fill(dt);
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (List): " + ex.Message, "Listar"); }
            return dt;
        }

        public int Insertar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    string query = @"INSERT INTO tblStaff (Name, LastName, idDepartment,Birthday, AppDay, HiredDay, EndDay, Address, IdGeoRegion, idGeoState, idGeoCity, ZipCod, Phone, Email, ContactPersonA, ContactPhoneA, ContactPersonB, ContactPhoneB, idDocsPayroll, CompanyName, idBanks, idBanksMethod, AccountNumber, RoutingNumber, idDocsInmigration, Social, JobDes, Rate, Active, Latitude, Longitude) 
                                     VALUES (@Name, @LastName, @idDepartment, @Birthday, @AppDay, @HiredDay, @EndDay, @Address, @IdGeoRegion, @idGeoState, @idGeoCity, @ZipCod, @Phone, @Email, @ContactPersonA, @ContactPhoneA, @ContactPersonB, @ContactPhoneB, @idDocsPayroll, @CompanyName, @idBanks, @idBanksMethod, @AccountNumber, @RoutingNumber, @idDocsInmigration, @Social, @JobDes, @Rate, @Active, @Latitude, @Longitud);
                                     SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SetParameters(cmd);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Insert): " + ex.Message); return 0; }
        }

        public bool Actualizar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    string query = @"UPDATE tblStaff SET Name=@Name, LastName=@LastName, idDepartment=@idDepartment, Birthday=@Birthday, AppDay=@AppDay, HiredDay=@HiredDay, EndDay=@EndDay, Address=@Address, IdGeoRegion=@IdGeoRegion, idGeoState=@idGeoState, idGeoCity=@idGeoCity, ZipCod=@ZipCod, Phone=@Phone, Email=@Email, ContactPersonA=@ContactPersonA, ContactPhoneA=@ContactPhoneA, ContactPersonB=@ContactPersonB, ContactPhoneB=@ContactPhoneB, idDocsPayroll=@idDocsPayroll, CompanyName=@CompanyName, idBanks=@idBanks, idBanksMethod=@idBanksMethod, AccountNumber=@AccountNumber, RoutingNumber=@RoutingNumber, idDocsInmigration=@idDocsInmigration, Social=@Social, JobDes=@JobDes, Rate=@Rate, Active=@Active, Latitude=@Latitude, Longitude=@Longitude
                                     WHERE id=@id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    SetParameters(cmd);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Update): " + ex.Message); return false; }
        }

        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    con.Open();
                    // Primero borramos idiomas por integridad
                    new SqlCommand($"DELETE FROM tblStaffLanguage WHERE idStaff = {idEliminar}", con).ExecuteNonQuery();
                    // Luego el staff
                    SqlCommand cmd = new SqlCommand("DELETE FROM tblStaff WHERE id = @id", con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Delete): " + ex.Message); return false; }
        }

        private void SetParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Name", (object)Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastName", (object)LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdDepartment", (object)IdDepartment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Birthday", (object)Birthday ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AppDay", (object)AppDay ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HiredDay", (object)HiredDay ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDay", (object)EndDay ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object)Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdGeoRegion", (object)IdGeoRegion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoState", (object)IdGeoState ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoCity", (object)IdGeoCity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ZipCod", (object)ZipCod ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactPersonA", (object)ContactPersonA ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactPhoneA", (object)ContactPhoneA ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactPersonB", (object)ContactPersonB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactPhoneB", (object)ContactPhoneB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idDocsPayroll", (object)IdDocsPayroll ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CompanyName", (object)CompanyName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idBanks", (object)IdBanks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idBanksMethod", (object)IdBanksMethod ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountNumber", (object)AccountNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoutingNumber", (object)RoutingNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idDocsInmigration", (object)IdDocsInmigration ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Social", (object)Social ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Rate", (object)Rate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@JobDes", (object)JobDes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Active", Active);
            cmd.Parameters.AddWithValue("@Latitude", (object)Latitude ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Longitude", (object)Longitude ?? DBNull.Value);
        }

        public void GuardarIdiomas(int idStaff, List<Human_Resources.Forms.LangStaff> listaIdiomas)
        {
            using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
            {
                con.Open();
                SqlTransaction tra = con.BeginTransaction();
                try
                {
                    // 1. Borrar idiomas anteriores para este empleado
                    using (SqlCommand cmdDel = new SqlCommand("DELETE FROM tblStaffLanguage WHERE idStaff = @id", con, tra))
                    {
                        cmdDel.Parameters.AddWithValue("@id", idStaff);
                        cmdDel.ExecuteNonQuery();
                    }

                    // 2. Insertar nuevos idiomas (Solo los que tengan al menos una opción marcada)
                    foreach (var idioma in listaIdiomas)
                    {
                        // Solo guardamos si el usuario marcó Speak, Read o Write
                        if (idioma.IsChecked || idioma.CanRead || idioma.CanWrite)
                        {
                            string query = @"INSERT INTO tblStaffLanguage (idStaff, idLanguage, CanWrite, CanRead) 
                                     VALUES (@s, @l, @cw, @cr)";

                            using (SqlCommand cmdIns = new SqlCommand(query, con, tra))
                            {
                                cmdIns.Parameters.AddWithValue("@s", idStaff);
                                cmdIns.Parameters.AddWithValue("@l", idioma.Id);
                                // Convertimos bool a int (1 o 0) para SQL
                                cmdIns.Parameters.AddWithValue("@cw", idioma.CanWrite ? 1 : 0);
                                cmdIns.Parameters.AddWithValue("@cr", idioma.CanRead ? 1 : 0);

                                cmdIns.ExecuteNonQuery();
                            }
                        }
                    }

                    tra.Commit();
                }
                catch (Exception ex)
                {
                    if (tra.Connection != null) tra.Rollback();
                    throw new Exception("Error saving languages details: " + ex.Message);
                }
            }
        }


        public DataTable ListarCatalogoIdiomas()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Nota: Asegúrate de que el nombre de la tabla sea "tblLanguages" en tu BD
                    string query = "SELECT id, Description FROM tblLanguages ORDER BY Description";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                // El 'throw' permite que el formulario (frmStaff) sepa que hubo un error
                // y podrías mostrar un MessageBox.Show(ex.Message);
                throw new Exception("Error listing language catalog:" + ex.Message);
            }
        }



        public DataTable ObtenerIdiomasDetallados(int idStaff)
        {
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                // Traemos el ID del idioma y los estados de lectura/escritura
                string query = "SELECT idLanguage, CanRead, CanWrite FROM tblStaffLanguage WHERE idStaff = @idStaff";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@idStaff", idStaff);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
        public class LicenseEntry
        {
            public int IdDepLicense { get; set; }
            public string Description { get; set; }
            public string LicenseNumber { get; set; }
        }

        public void GuardarLicencias(int idStaff, List<LicenseEntry> licencias)
        {
            using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
            {
                con.Open();
                SqlTransaction tra = con.BeginTransaction();
                try
                {
                    // Borramos lo anterior para que si el usuario borró un número de licencia, 
                    // este desaparezca también de la tabla.
                    SqlCommand cmdDel = new SqlCommand("DELETE FROM tblStaffLicense WHERE idStaff = @id", con, tra);
                    cmdDel.Parameters.AddWithValue("@id", idStaff);
                    cmdDel.ExecuteNonQuery();

                    foreach (var lic in licencias)
                    {
                        // Aunque ya filtramos en el Form, esta validación doble es seguridad de Sensei
                        if (!string.IsNullOrWhiteSpace(lic.LicenseNumber))
                        {
                            string queryIns = "INSERT INTO tblStaffLicense (idStaff, idDepLicense, LicenseNumber) VALUES (@s, @l, @n)";
                            SqlCommand cmdIns = new SqlCommand(queryIns, con, tra);
                            cmdIns.Parameters.AddWithValue("@s", idStaff);
                            cmdIns.Parameters.AddWithValue("@l", lic.IdDepLicense);
                            cmdIns.Parameters.AddWithValue("@n", lic.LicenseNumber);
                            cmdIns.ExecuteNonQuery();
                        }
                    }
                    tra.Commit();
                }
                catch (Exception) { tra.Rollback(); throw; }
            }
        }

        public List<LicenseEntry> ObtenerLicenciasPorEmpleado(int idStaff, int? idDept)
        {
            List<LicenseEntry> lista = new List<LicenseEntry>();
            if (idDept == null) return lista;

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                // El SQL se queda encapsulado aquí, en la capa de datos
                string query = @"SELECT L.id, L.Description, SL.LicenseNumber 
                         FROM tblDepLicense L 
                         LEFT JOIN tblStaffLicense SL ON L.id = SL.idDepLicense AND SL.idStaff = @idStaff
                         WHERE L.idDepartment = @idDept";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@idStaff", idStaff);
                cmd.Parameters.AddWithValue("@idDept", idDept);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new LicenseEntry
                    {
                        IdDepLicense = Convert.ToInt32(dr["id"]),
                        Description = dr["Description"].ToString(),
                        LicenseNumber = dr["LicenseNumber"]?.ToString() ?? ""
                    });
                }
            }
            return lista;
        }

        // Agrega este método dentro de ClassStaff.cs
        public DataTable FiltrarPersonal(int idDepartment, string name, int idRegion, bool soloActivos, int idState, int idCity, string zip, int idLicense, int idPayroll, int idBank, int idMethod, int idInmigration)
        {
            DataTable dt = new DataTable();
            // Iniciamos con el filtro base de departamento
            string query = "SELECT * FROM tblStaff S WHERE S.idDepartment = @idDept";

            // --- FILTRO DE ESTADO ACTIVO ---
            // Si 'soloActivos' es true, filtramos solo por 1.
            // Si es false, no añadimos filtro para que traiga activos, inactivos y nulos.
            query += " AND ISNULL(S.Active, 0) = " + (soloActivos ? "1" : "0");

            // Filtros dinámicos de texto y IDs
            if (!string.IsNullOrEmpty(name)) query += " AND (S.Name LIKE @name OR S.LastName LIKE @name)";
            if (idRegion > 0) query += " AND S.idGeoRegion = @idRegion";
            if (idState > 0) query += " AND S.idGeoState = @idState";
            if (idCity > 0) query += " AND S.idGeoCity = @idCity";
            if (!string.IsNullOrEmpty(zip)) query += " AND S.ZipCod = @zip";
            if (idPayroll > 0) query += " AND S.idDocsPayroll = @idPayroll";
            if (idBank > 0) query += " AND S.idBanks = @idBank";
            if (idMethod > 0) query += " AND S.idBanksMethod = @idMethod";
            if (idInmigration > 0) query += " AND S.idDocsInmigration = @idInmigration";

            // La cirugía de la Licencia
            if (idLicense > 0)
            {
                query += " AND EXISTS (SELECT 1 FROM tblStaffLicense SL WHERE SL.idStaff = S.id AND SL.idDepLicense = @idLicense)";
            }

            query += " ORDER BY LastName, Name";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@idDept", idDepartment);
                cmd.Parameters.AddWithValue("@name", "%" + name + "%");
                cmd.Parameters.AddWithValue("@idRegion", idRegion);
                cmd.Parameters.AddWithValue("@idState", idState);
                cmd.Parameters.AddWithValue("@idCity", idCity);
                cmd.Parameters.AddWithValue("@zip", zip ?? "");
                cmd.Parameters.AddWithValue("@idLicense", idLicense);
                cmd.Parameters.AddWithValue("@idPayroll", idPayroll);
                cmd.Parameters.AddWithValue("@idBank", idBank);
                cmd.Parameters.AddWithValue("@idMethod", idMethod);
                cmd.Parameters.AddWithValue("@idInmigration", idInmigration);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }






        public DataTable XXXFiltrarPersonal(int idDepartment, string name, int idRegion, bool soloActivos, int idState, int idCity, string zip, int idLicense, int idPayroll, int idBank, int idMethod, int idInmigration)
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM tblStaff S WHERE S.idDepartment = @idDept";

            // Filtros dinámicos
            if (soloActivos) query += " AND S.Active = 1";
            if (!string.IsNullOrEmpty(name)) query += " AND (S.Name LIKE @name OR S.LastName LIKE @name)";
            if (idRegion > 0) query += " AND S.idGeoRegion = @idRegion";
            if (idState > 0) query += " AND S.idGeoState = @idState";
            if (idCity > 0) query += " AND S.idGeoCity = @idCity";
            if (!string.IsNullOrEmpty(zip)) query += " AND S.ZipCod = @zip";
            if (idPayroll > 0) query += " AND S.idDocsPayroll = @idPayroll";
            if (idBank > 0) query += " AND S.idBanks = @idBank";
            if (idMethod > 0) query += " AND S.idBanksMethod = @idMethod";
            if (idInmigration > 0) query += " AND S.idDocsInmigration = @idInmigration";

            // La cirugía de la Licencia (WHERE EXISTS)
            if (idLicense > 0)
            {
                query += " AND EXISTS (SELECT 1 FROM tblStaffLicense SL WHERE SL.idStaff = S.id AND SL.idDepLicense = @idLicense)";
            }

            query += " ORDER BY LastName, Name";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@idDept", idDepartment);
                cmd.Parameters.AddWithValue("@name", "%" + name + "%");
                cmd.Parameters.AddWithValue("@idRegion", idRegion);
                cmd.Parameters.AddWithValue("@idState", idState);
                cmd.Parameters.AddWithValue("@idCity", idCity);
                cmd.Parameters.AddWithValue("@zip", zip ?? "");
                cmd.Parameters.AddWithValue("@idLicense", idLicense);
                cmd.Parameters.AddWithValue("@idPayroll", idPayroll);
                cmd.Parameters.AddWithValue("@idBank", idBank);
                cmd.Parameters.AddWithValue("@idMethod", idMethod);
                cmd.Parameters.AddWithValue("@idInmigration", idInmigration);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }
}