using System;
using System.Configuration; // Necesario para ConfigurationManager
using System.Data.SqlClient; // Necesario para SqlConnectionStringBuilder

namespace Human_Resources.Data
{
    public static class ClassConexion
    {
        public static string CadenaConexion
        {
            get
            {
                // Lee el entorno desde appSettings
                string currentEnvironment = ConfigurationManager.AppSettings["CurrentEnvironment"];

                string connectionName;

                // Decide qué cadena de conexión usar basada en el entorno
                if (string.Equals(currentEnvironment, "Azure", StringComparison.OrdinalIgnoreCase))
                {
                    connectionName = "AzureDbConnection";
                }
                else if (string.Equals(currentEnvironment, "GoogleCloud", StringComparison.OrdinalIgnoreCase)) // Si decides usar Google Cloud
                {
                    connectionName = "GoogleCloudDbConnection";
                }
                else // Por defecto, o si la clave no está configurada o es "Local"
                {
                    connectionName = "LocalDbConnection";
                }

                // Asegúrate de que la cadena de conexión exista
                string baseConnectionString = GetConnectionString(connectionName);

                // Construye la cadena de conexión con seguridades adicionales
                return BuildSecureConnectionString(baseConnectionString, currentEnvironment);
            }
        }

        // Método auxiliar para obtener la cadena de conexión sin procesar del App.config
        private static string GetConnectionString(string name)
        {
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[name];
            if (connectionStringSettings == null || string.IsNullOrEmpty(connectionStringSettings.ConnectionString))
            {
                throw new ConfigurationErrorsException($"Connection string '{name}' is missing or empty in App.config for the current environment.");
            }
            return connectionStringSettings.ConnectionString;
        }

        // *** CAPA DE SEGURIDAD MEJORADA ***
        // Ahora el método BuildSecureConnectionString también recibe el entorno
        private static string BuildSecureConnectionString(string baseConnectionString, string environment)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(baseConnectionString);

            // Configuraciones de seguridad comunes a todos los entornos, pero especialmente importantes para la nube
            builder.ConnectTimeout = 30; // 30 segundos, puedes ajustar
            builder.MultipleActiveResultSets = false;

            // Configuraciones específicas para entornos en la nube (Azure/Google Cloud)
            // Estas sobrescribirán cualquier cosa en la cadena base para asegurar la seguridad.
            if (string.Equals(environment, "Azure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(environment, "GoogleCloud", StringComparison.OrdinalIgnoreCase))
            {
                builder.Encrypt = true;
                builder.TrustServerCertificate = false; // Requiere que el certificado sea válido y confiable
                                                        // Para Azure/Google Cloud SQL, esto es lo común
            }
            // Para el entorno "Local", podemos mantener los valores que vienen en la cadena base
            // (a menudo Encrypt=False o TrustServerCertificate=True para LocalDB/Express)

            return builder.ToString();
        }
    }
}