using System;
using System.Collections.Generic;
using System.Configuration; // Para ConfigurationManager si lo usaras, aunque ya está en ClassConexion
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography; // ¡IMPORTANTE para el hashing!
using System.Text;
using System.Windows; // Para MessageBox, aunque es mejor que las capas de datos no lo usen directamente.
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;


namespace Human_Resources.Data
{
    
    // Clase para representar un módulo de acceso en C#
    public class AccessModule
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsChecked { get; set; } // Para la UI del ListBox
    }
    
    public class ClassCfgUsers
    {

        // OBTENEMOS LA VERSION DE LA APLICACI
        public static string ObtenerVersionUpdate()
        {
            // 1. Definimos la ruta de la "Calle Principal" en la unidad G
            string rutaInstalador = @"G:\Mi unidad\Software_Deployments\Human_Resources\Installer\Human Resources.application";

            try
            {
                // 2. ¿Existe el archivo? Si no, salimos sin hacer ruido
                if (!File.Exists(rutaInstalador)) return null;

                // 3. Leemos el XML del archivo .application
                XDocument xmlDoc = XDocument.Load(rutaInstalador);
                XNamespace asmv1 = "urn:schemas-microsoft-com:asm.v1";

                // 4. Buscamos la etiqueta <assemblyIdentity> que tiene la versión
                var assemblyIdentity = xmlDoc.Descendants(asmv1 + "assemblyIdentity").FirstOrDefault();

                if (assemblyIdentity != null)
                {
                    return assemblyIdentity.Attribute("version")?.Value;
                }
            }
            catch (Exception ex)
            {
                // Solo para depuración en Visual Studio
                Debug.WriteLine("Error leyendo versión en Drive: " + ex.Message);
            }
            return null;
        }


        // --- PROPIEDADES DE USUARIO ---
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DescriptionJob { get; set; } // Puesto / Descripción del trabajo
        public bool Active { get; set; }

        // La contraseña real (en texto plano) solo se usa para hashear/verificar
        // NUNCA la guardes en esta propiedad después de hashear
        public string PasswordText { get; set; }

        // La clave hasheada y salteada se leerá y escribirá directamente desde la DB
        private string _claveHash; // Campo privado para la clave hasheada
        public string ClaveHash
        {
            get { return _claveHash; }
            set { _claveHash = value; }
        }

        // --- LISTA ESTÁTICA DE MÓDULOS DE ACCESO (Desde el código C#) ---
        public static List<AccessModule> AllAccessModules = new List<AccessModule>
        {
            new AccessModule { Id = 1, Description = "Definitions", IsChecked = false },
            new AccessModule { Id = 2, Description = "Documents", IsChecked = false },
            new AccessModule { Id = 3, Description = "Company Staff", IsChecked = false },
            new AccessModule { Id = 4, Description = "Post Documents", IsChecked = false },
            new AccessModule { Id = 5, Description = "Expired Docs", IsChecked = false },
            new AccessModule { Id = 5, Description = "Prospects", IsChecked = false },
            new AccessModule { Id = 6, Description = "Setup", IsChecked = false }
        };

        // --- MÉTODOS DE SEGURIDAD (PASSWORD HASHING) ---

        // Configuración para el hashing (puedes ajustarlos, pero estos son buenos valores por defecto)
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 20; // 160 bits
        private const int Iterations = 10000; // Número de iteraciones

        /// <summary>
        /// Genera un hash seguro con salt para una contraseña.
        /// El formato de retorno es "salt (base64).hash (base64)".
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be empty.");
            }

            // 1. Generar un salt aleatorio usando la clase moderna
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // 2. Crear el hash con PBKDF2 (El resto sigue igual)
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // 3. Combinar salt y hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifica una contraseña contra un hash almacenado.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            try
            {
                // 1. Convertir el hash almacenado de Base64 a bytes
                byte[] hashBytes = Convert.FromBase64String(storedHash);

                // 2. Extraer el salt del hash almacenado (los primeros 16 bytes)
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // 3. Crear el hash con la contraseña que escribió el usuario y el salt extraído
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // 4. Comparar los hashes byte por byte
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                    {
                        return false; // No coincide
                    }
                }
                return true; // ¡Coincide!
            }
            catch
            {
                // Si el hash en la DB no tiene el formato correcto (Base64), llegará aquí
                return false;
            }
        }



        // --- MÉTODOS CRUD BÁSICOS PARA USUARIOS ---

        /// <summary>
        /// Lista todos los usuarios (o filtra por nombre/apellido).
        /// </summary>
        public DataTable Listar(string filtro = "")
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT id, FullName, Name, email, phone, DescriptionJob, Active FROM cfgUsers WHERE FullName LIKE @f ORDER BY FullName";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.SelectCommand.Parameters.AddWithValue("@f", "%" + filtro + "%");
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving users: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dt;
        }

        /// <summary>
        /// Inserta un nuevo usuario y devuelve su ID.
        /// </summary>
        public int Insertar()
        {
            try
            {
                // Hashear la contraseña antes de insertar
                this.ClaveHash = HashPassword(this.PasswordText);

                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"INSERT INTO cfgUsers (FullName, Name, Clave, email, phone, DescriptionJob, Active)
                                     VALUES (@FullName, @Name, @Clave, @email, @phone, @DescriptionJob, @Active);
                                     SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SetParameters(cmd); // Usamos el helper para los parámetros
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()); // Retorna el ID del nuevo usuario
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting user: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        /// <summary>
        /// Actualiza un usuario existente. Si PasswordText tiene valor, también actualiza la contraseña.
        /// </summary>
        public bool Actualizar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = @"UPDATE cfgUsers SET FullName=@FullName, Name=@Name, email=@email, phone=@phone, DescriptionJob=@DescriptionJob, Active=@Active";

                    // Solo actualiza la contraseña si se proporcionó una nueva
                    if (!string.IsNullOrEmpty(this.PasswordText))
                    {
                        this.ClaveHash = HashPassword(this.PasswordText);
                        query += ", Clave=@Clave";
                    }
                    query += " WHERE id=@id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    SetParameters(cmd, updatePassword: !string.IsNullOrEmpty(this.PasswordText)); // Pasa true si se actualiza la contraseña
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating user: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Elimina un usuario por ID.
        /// </summary>
        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Debido al ON DELETE CASCADE en cfgAccess, al eliminar el usuario,
                    // sus registros de cfgAccess también se eliminan automáticamente.
                    string query = "DELETE FROM cfgUsers WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting user: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Obtiene los detalles de un usuario por su ID.
        /// </summary>
        public bool ObtenerPorId(int idBusqueda)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT id, FullName, Name, Clave, email, phone, DescriptionJob, Active FROM cfgUsers WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idBusqueda);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        this.Id = Convert.ToInt32(dr["id"]);
                        this.FullName = dr["FullName"].ToString();
                        this.Name = dr["Name"].ToString();
                        this.ClaveHash = dr["Clave"].ToString(); // Guardamos el hash para verificación
                        this.Email = dr["email"].ToString();
                        this.Phone = dr["phone"].ToString();
                        this.DescriptionJob = dr["DescriptionJob"].ToString();
                        this.Active = Convert.ToBoolean(dr["Active"]);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user details: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        /// <summary>
        /// Intenta autenticar a un usuario. Devuelve el ID del usuario si las credenciales son correctas, de lo contrario 0.
        /// </summary>
        public int Login(string Name, string password)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    // Usamos TRIM para evitar errores de espacios en blanco
                    string query = "SELECT id, Clave, Active FROM cfgUsers WHERE LTRIM(RTRIM(Name)) = @Name";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Name", Name.Trim());

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        // SOLUCIÓN AL BIT: Manejo seguro del booleano
                        object activeValue = dr["Active"];
                        bool isActive = (activeValue != DBNull.Value && Convert.ToBoolean(activeValue));

                        if (!isActive)
                        {
                            MessageBox.Show("La cuenta de usuario está inactiva.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return 0;
                        }

                        string storedHash = dr["Clave"].ToString();

                        // VERIFICACIÓN:
                        if (VerifyPassword(password, storedHash))
                        {
                            return Convert.ToInt32(dr["id"]);
                        }
                        else
                        {
                            // Esto te ayudará a saber si el usuario existe pero la clave está mal
                            Console.WriteLine("DEBUG: Contraseña incorrecta o Hash incompatible.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("DEBUG: No se encontró ningún usuario con ese nombre.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el login: " + ex.Message);
            }
            return 0;
        }


        // --- MÉTODOS PARA GESTIÓN DE ACCESOS ---

        /// <summary>
        /// Guarda los módulos de acceso seleccionados para un usuario.
        /// </summary>
        public void GuardarAccesos(int idUsuario, List<AccessModule> accesosSeleccionados)
        {
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                con.Open();
                SqlTransaction tra = con.BeginTransaction();
                try
                {
                    // 1. Eliminar accesos existentes para este usuario
                    using (SqlCommand cmdDel = new SqlCommand("DELETE FROM cfgAccess WHERE idUser = @idUser", con, tra))
                    {
                        cmdDel.Parameters.AddWithValue("@idUser", idUsuario);
                        cmdDel.ExecuteNonQuery();
                    }

                    // 2. Insertar solo los accesos marcados
                    foreach (var modulo in accesosSeleccionados)
                    {
                        if (modulo.IsChecked)
                        {
                            string queryIns = "INSERT INTO cfgAccess (idUser, Access) VALUES (@idUser, @Access)";
                            using (SqlCommand cmdIns = new SqlCommand(queryIns, con, tra))
                            {
                                cmdIns.Parameters.AddWithValue("@idUser", idUsuario);
                                cmdIns.Parameters.AddWithValue("@Access", modulo.Id);
                                cmdIns.ExecuteNonQuery();
                            }
                        }
                    }
                    tra.Commit();
                }
                catch (Exception ex)
                {
                    if (tra.Connection != null) tra.Rollback();
                    MessageBox.Show("Error saving user access: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw; // Re-lanza la excepción para que el formulario pueda manejarla
                }
            }
        }

        /// <summary>
        /// Obtiene los AccessModuleIds que tiene asignados un usuario.
        /// </summary>
        public List<int> ObtenerAccesosPorUsuario(int idUsuario)
        {
            List<int> accesos = new List<int>();
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT Access FROM cfgAccess WHERE idUser = @idUser";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@idUser", idUsuario);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        accesos.Add(Convert.ToInt32(dr["Access"]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving user access: " + ex.Message, "ObtenerAccesosPorUsuario", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return accesos;
        }


        // --- MÉTODO HELPER PARA PARÁMETROS SQL ---

        /// <summary>
        /// Establece los parámetros comunes para INSERT y UPDATE.
        /// </summary>
        private void SetParameters(SqlCommand cmd, bool updatePassword = false)
        {
            cmd.Parameters.AddWithValue("@FullName", (object)FullName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Name", (object)Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object)Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object)Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DescriptionJob", (object)DescriptionJob ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Active", Active);

            // Solo añadir el parámetro @Clave si se va a actualizar la contraseña
            if (updatePassword || cmd.CommandText.Contains("INSERT INTO")) // Para INSERT, siempre se necesita la clave
            {
                cmd.Parameters.AddWithValue("@Clave", (object)ClaveHash ?? DBNull.Value);
            }
        }

        // Añade este método al final de tu clase ClassCfgUsers
        public static string GetSha256Hash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }
}