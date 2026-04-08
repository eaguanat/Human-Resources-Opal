using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Human_Resources.Data
{
    public static class ClassConexion
    {
        public static string CadenaConexion
        {
            get
            {
                // COMENTA ESTA LÍNEA CUANDO REALMENTE QUIERAS IR A AZURE
                // Por ahora, esto obliga al sistema a usar LOCAL sí o sí.
                return GetConnectionString("LocalDbConnection");

                /* #if DEBUG
                    return GetConnectionString("LocalDbConnection");
                #else
                    return GetConnectionString("AzureDbConnection");
                #endif
                */
            }
        }



        private static string GetConnectionString(string name)
        {
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[name];
            if (connectionStringSettings == null || string.IsNullOrEmpty(connectionStringSettings.ConnectionString))
            {
                throw new ConfigurationErrorsException($"Connection string '{name}' is missing or empty in App.config.");
            }
            return connectionStringSettings.ConnectionString;
        }

        private static string BuildSecureConnectionString(string baseConnectionString, string environment)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(baseConnectionString);

            builder.ConnectTimeout = 30;
            builder.MultipleActiveResultSets = false;

            // Si detectamos que NO es local, blindamos la conexión para la nube
            if (environment == "Azure")
            {
                builder.Encrypt = true;
                builder.TrustServerCertificate = false;
            }

            return builder.ToString();
        }
    }
}