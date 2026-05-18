using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows; // Para MessageBox (idealmente la capa de datos no debería usarlos)

namespace Human_Resources.Data
{
    // Clase auxiliar para los estados del aplicante
    public class ApplicantStatus
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class ClassApplicants
    {
        // --- PROPIEDADES ---
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordText { get; set; } // Para entrada/actualización de contraseña
        public string PasswordHash { get; set; } // Para almacenamiento
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? IdDepartment { get; set; }
        public int? IdGeoState { get; set; }
        public int? IdGeoCity { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public int Status { get; set; } // No nulo, con default
        public DateTime DateCreated { get; set; }
        public string ServiceZipCodes { get; set; }
        public string Observations { get; set; }

        // --- LISTA ESTÁTICA DE ESTADOS DE APLICANTE ---
        public static List<ApplicantStatus> AllApplicantStatuses = new List<ApplicantStatus>
        {
            new ApplicantStatus { Id = 1, Description = "NOT REVIEWED" },
            new ApplicantStatus { Id = 2, Description = "REVIEWED - RELEVANT" },
            new ApplicantStatus { Id = 3, Description = "REVIEWED - NOT RELEVANT" },
            new ApplicantStatus { Id = 4, Description = "UNDER REVIEW" },
            new ApplicantStatus { Id = 5, Description = "PENDING DOCUMENTS" }
        };

        // --- MÉTODOS CRUD Y ESPECÍFICOS ---

        /// <summary>
        /// Lista aspirantes con filtros dinámicos.
        /// </summary>
        public DataTable ListarFiltrado(int? idDepartment, int? idGeoState, int? idGeoCity, int? status,
                                                string zipCode, string serviceZipCodes, string fullName)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    StringBuilder query = new StringBuilder(@"
                        SELECT 
                            A.Id, A.FirstName, A.LastName, A.Email, A.Phone, A.Status, A.DateCreated,
                            D.Description AS DepartmentName, 
                            S.Description AS StateName, 
                            C.Description AS CityName
                        FROM tblApplicants A
                        LEFT JOIN tblDepartment D ON A.idDepartment = D.Id
                        LEFT JOIN tblGeoState S ON A.idGeoState = S.Id
                        LEFT JOIN tblGeoCity C ON A.idGeoCity = C.Id
                        WHERE 1=1 "); // Condición base siempre verdadera para facilitar filtros dinámicos

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    // Filtros obligatorios y opcionales (el resto del código de filtros es igual)
                    if (idDepartment.HasValue && idDepartment.Value > 0)
                    {
                        query.Append(" AND A.idDepartment = @idDepartment");
                        cmd.Parameters.AddWithValue("@idDepartment", idDepartment.Value);
                    }
                    if (idGeoState.HasValue && idGeoState.Value > 0)
                    {
                        query.Append(" AND A.idGeoState = @idGeoState");
                        cmd.Parameters.AddWithValue("@idGeoState", idGeoState.Value);
                    }
                    if (idGeoCity.HasValue && idGeoCity.Value > 0)
                    {
                        query.Append(" AND A.idGeoCity = @idGeoCity");
                        cmd.Parameters.AddWithValue("@idGeoCity", idGeoCity.Value);
                    }
                    if (status.HasValue && status.Value > 0)
                    {
                        query.Append(" AND A.Status = @Status");
                        cmd.Parameters.AddWithValue("@Status", status.Value);
                    }
                    if (!string.IsNullOrWhiteSpace(zipCode))
                    {
                        query.Append(" AND A.ZipCode LIKE @ZipCode");
                        cmd.Parameters.AddWithValue("@ZipCode", "%" + zipCode + "%");
                    }
                    if (!string.IsNullOrWhiteSpace(serviceZipCodes))
                    {
                        query.Append(" AND A.ServiceZipCodes LIKE @ServiceZipCodes");
                        cmd.Parameters.AddWithValue("@ServiceZipCodes", "%" + serviceZipCodes + "%");
                    }
                    if (!string.IsNullOrWhiteSpace(fullName))
                    {
                        query.Append(" AND (A.FirstName LIKE @FullName OR A.LastName LIKE @FullName)");
                        cmd.Parameters.AddWithValue("@FullName", "%" + fullName + "%");
                    }

                    query.Append(" ORDER BY A.LastName, A.FirstName");
                    cmd.CommandText = query.ToString();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    // --- ¡NUEVA LÓGICA AQUÍ PARA CONVERTIR EL STATUS ID A DESCRIPCIÓN! ---
                    if (dt.Columns.Contains("Status") && !dt.Columns.Contains("StatusDescription"))
                    {
                        dt.Columns.Add("StatusDescription", typeof(string));
                        foreach (DataRow row in dt.Rows)
                        {
                            int currentStatusId = Convert.ToInt32(row["Status"]);
                            string statusDesc = AllApplicantStatuses
                                                .FirstOrDefault(s => s.Id == currentStatusId)?
                                                .Description ?? "Unknown Status";
                            row["StatusDescription"] = statusDesc;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving applicants: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dt;
        }
        /// <summary>
        /// Inserta un nuevo aspirante.
        /// </summary>
        public int Insertar()
        {
            try
            {
                // Hashear la contraseña antes de insertar
                // this.PasswordHash = ClassCfgUsers.HashPassword(this.PasswordText);
                this.PasswordHash = this.PasswordText;

                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"INSERT INTO tblApplicants (Email, Password, FirstName, LastName, idDepartment, idGeoState, idGeoCity, Address, ZipCode, Phone, Status, DateCreated, ServiceZipCodes, Observations)
                                     VALUES (@Email, @Password, @FirstName, @LastName, @idDepartment, @idGeoState, @idGeoCity, @Address, @ZipCode, @Phone, @Status, @DateCreated, @ServiceZipCodes, @Observations);
                                     SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SetAllParameters(cmd);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Error de clave única (Email ya existe)
                {
                    MessageBox.Show("An applicant with this email already exists.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Error inserting applicant: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting applicant: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        /// <summary>
        /// Obtiene los detalles completos de un aspirante por ID.
        /// </summary>
        public bool ObtenerPorId(int idBusqueda)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"
                        SELECT 
                            A.*, 
                            D.Description AS DepartmentName, 
                            S.Description AS StateName, 
                            C.Description AS CityName
                        FROM tblApplicants A
                        LEFT JOIN tblDepartment D ON A.idDepartment = D.Id
                        LEFT JOIN tblGeoState S ON A.idGeoState = S.Id
                        LEFT JOIN tblGeoCity C ON A.idGeoCity = C.Id
                        WHERE A.Id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idBusqueda);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        this.Id = Convert.ToInt32(dr["Id"]);
                        this.Email = dr["Email"].ToString();
                        this.PasswordText = dr["Password"].ToString(); // Solo el hash, no la contraseña original
                        this.FirstName = dr["FirstName"].ToString();
                        this.LastName = dr["LastName"].ToString();
                        this.IdDepartment = dr["idDepartment"] != DBNull.Value ? Convert.ToInt32(dr["idDepartment"]) : (int?)null;
                        this.IdGeoState = dr["idGeoState"] != DBNull.Value ? Convert.ToInt32(dr["idGeoState"]) : (int?)null;
                        this.IdGeoCity = dr["idGeoCity"] != DBNull.Value ? Convert.ToInt32(dr["idGeoCity"]) : (int?)null;
                        this.Address = dr["Address"].ToString();
                        this.ZipCode = dr["ZipCode"].ToString();
                        this.Phone = dr["Phone"].ToString();
                        this.Status = Convert.ToInt32(dr["Status"]);
                        this.DateCreated = Convert.ToDateTime(dr["DateCreated"]);
                        this.ServiceZipCodes = dr["ServiceZipCodes"].ToString();
                        this.Observations = dr["Observations"].ToString();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading applicant details: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        /// <summary>
        /// Actualiza SOLO el Status de un aspirante.
        /// </summary>
        public bool ActualizarStatus()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "UPDATE tblApplicants SET Status = @Status WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Status", this.Status);
                    cmd.Parameters.AddWithValue("@Id", this.Id);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating applicant status: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Elimina un aspirante por ID.
        /// </summary>
        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "DELETE FROM tblApplicants WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting applicant: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Helper para establecer TODOS los parámetros para INSERT/UPDATE completo (no solo Status).
        /// </summary>
        private void SetAllParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Password", (object)PasswordHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstName", (object)FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastName", (object)LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idDepartment", (object)IdDepartment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoState", (object)IdGeoState ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoCity", (object)IdGeoCity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object)Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ZipCode", (object)ZipCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", Status);
            cmd.Parameters.AddWithValue("@DateCreated", DateCreated);
            cmd.Parameters.AddWithValue("@ServiceZipCodes", (object)ServiceZipCodes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Observations", (object)Observations ?? DBNull.Value);
        }

        /// <summary>
        /// Mueve un aplicante a la tabla de Staff (como inactivo) y lo elimina de la lista de aplicantes.
        /// Utiliza una transacción para asegurar la integridad de los datos.
        /// </summary>
        public bool PromoverAStaff(int idAplicante)
        {
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction(); // Iniciamos la transacción

                try
                {
                    // 1. Primero obtenemos los datos del aplicante (asegurándonos de tenerlos frescos)
                    // Reutilizamos el query de ObtenerPorId pero asociado a la transacción
                    string selectQuery = "SELECT * FROM tblApplicants WHERE Id = @Id";
                    SqlCommand cmdSelect = new SqlCommand(selectQuery, con, transaction);
                    cmdSelect.Parameters.AddWithValue("@Id", idAplicante);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmdSelect))
                    {
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count == 0) throw new Exception("Applicant not found.");

                    DataRow row = dt.Rows[0];

                    // 2. Insertar en tblStaff 
                    // Nota: Usamos 'Active' (bit) en lugar de 'Status' y mapeamos los nombres correctos.
                    string insertQuery = @"
                                        INSERT INTO tblStaff 
                                        (Name, LastName, IdDepartment, Address, idGeoState, idGeoCity, ZipCod, Phone, Email, Active, HiredDay, Latitude, Longitude)
                                        VALUES 
                                        (@Name, @LastName, @IdDepartment, @Address, @idGeoState, @idGeoCity, @ZipCod, @Phone, @Email, 0, GETDATE(), @Latitude, @Longitude)";

                    SqlCommand cmdInsert = new SqlCommand(insertQuery, con, transaction);

                    // Mapeo: @Parametro -> row["Nombre_En_Tabla_Applicants"]
                    cmdInsert.Parameters.AddWithValue("@Name", row["FirstName"]); // En Staff es 'Name', en Applicants es 'FirstName'
                    cmdInsert.Parameters.AddWithValue("@LastName", row["LastName"]);
                    cmdInsert.Parameters.AddWithValue("@IdDepartment", row["idDepartment"]);
                    cmdInsert.Parameters.AddWithValue("@Address", row["Address"]);
                    cmdInsert.Parameters.AddWithValue("@idGeoState", row["idGeoState"]);
                    cmdInsert.Parameters.AddWithValue("@idGeoCity", row["idGeoCity"]);
                    cmdInsert.Parameters.AddWithValue("@ZipCod", row["ZipCode"]); // En Staff es 'ZipCod', en Applicants es 'ZipCode'
                    cmdInsert.Parameters.AddWithValue("@Phone", row["Phone"]);
                    cmdInsert.Parameters.AddWithValue("@Email", row["Email"]);
                    cmdInsert.Parameters.AddWithValue("@Latitude", row["Latitude"]);
                    cmdInsert.Parameters.AddWithValue("@Longitude", row["Longitude"]);

                    // Importante: 'Active' es bit. 0 = Inactivo (False).
                    cmdInsert.Parameters.AddWithValue("@Active", 0);

                    cmdInsert.ExecuteNonQuery();

                    // 3. Eliminar de tblApplicants
                    string deleteQuery = "DELETE FROM tblApplicants WHERE Id = @Id";
                    SqlCommand cmdDelete = new SqlCommand(deleteQuery, con, transaction);
                    cmdDelete.Parameters.AddWithValue("@Id", idAplicante);
                    cmdDelete.ExecuteNonQuery();

                    // Si todo salió bien, confirmamos los cambios
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    // Si algo falla, deshacemos todo lo que se haya hecho en esta conexión
                    transaction.Rollback();
                    MessageBox.Show("Promotion failed: " + ex.Message, "Transaction Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }
    }
}