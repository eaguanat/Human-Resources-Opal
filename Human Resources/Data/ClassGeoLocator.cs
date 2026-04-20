using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; // Para MessageBox, aunque idealmente en una capa de datos no se debería.


namespace Human_Resources.Data
{
    // Modelo para los resultados de la búsqueda geográfica
    public class GeoResult
    {
        public int Id { get; set; }
        public string EntityType { get; set; } // "Staff" o "Applicant"
        public string Name { get; set; } // FirstName + LastName o FullName
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double DistanceMiles { get; set; } // Distancia calculada en millas
        public string DepartmentName { get; set; }
        public string Phone { get; set; } // Añadimos teléfono para mostrar en el DataGrid
        public string Email { get; set; } // Añadimos email para mostrar en el DataGrid
    }

    public class ClassGeoLocator
    {
        // Radio de la Tierra en millas (aproximado)
        private const double EarthRadiusMiles = 3958.8;

        /// <summary>
        /// Obtiene los Staff o Applicants más cercanos a un punto geográfico, filtrados por departamento.
        /// </summary>
        /// <param name="entityType">"Staff" o "Applicant"</param>
        /// <param name="idDepartment">ID del departamento para filtrar (0 o null para todos)</param>
        /// <param name="centerLat">Latitud del punto central de búsqueda</param>
        /// <param name="centerLng">Longitud del punto central de búsqueda</param>
        /// <param name="topX">Número máximo de resultados a devolver</param>
        /// <returns>Lista de GeoResult ordenados por distancia.</returns>
        public List<GeoResult> GetNearestEntities(string entityType, int? idDepartment, decimal centerLat, decimal centerLng, int topX)
        {
            List<GeoResult> results = new List<GeoResult>();

            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    StringBuilder queryBuilder = new StringBuilder();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    string degToRad = " (PI() / 180.0) ";
                    string haversineFormula = $@"
                {EarthRadiusMiles} * ACOS(
                    pmin(1.0, pmax(-1.0, 
                    COS(@radCenterLat) * COS(Latitude * {degToRad}) *
                    COS(Longitude * {degToRad} - @radCenterLng) +
                    SIN(@radCenterLat) * SIN(Latitude * {degToRad})
                    ))
                )";

                    cmd.Parameters.AddWithValue("@radCenterLat", (double)centerLat * (Math.PI / 180.0));
                    cmd.Parameters.AddWithValue("@radCenterLng", (double)centerLng * (Math.PI / 180.0));
                    cmd.Parameters.AddWithValue("@topX", topX);

                    if (entityType == "Staff")
                    {
                        // CORRECCIÓN: En tblStaff usamos [Name] + [LastName]
                        queryBuilder.Append($@"
                    SELECT TOP (@topX)
                        S.Id,
                        'Staff' AS EntityType,
                        ISNULL(S.Name, '') + ' ' + ISNULL(S.LastName, '') AS Name,
                        S.Address,
                        S.Latitude,
                        S.Longitude,
                        ({haversineFormula}) AS DistanceMiles,
                        D.Description AS DepartmentName,
                        S.Phone,
                        S.Email
                    FROM tblStaff S
                    LEFT JOIN tblDepartment D ON S.idDepartment = D.Id
                    WHERE S.Latitude IS NOT NULL AND S.Longitude IS NOT NULL
                ");
                    }
                    else if (entityType == "Applicant")
                    {
                        // CORRECCIÓN: En tblApplicants usamos [FirstName] + [LastName]
                        queryBuilder.Append($@"
                    SELECT TOP (@topX)
                        A.Id,
                        'Applicant' AS EntityType,
                        ISNULL(A.FirstName, '') + ' ' + ISNULL(A.LastName, '') AS Name,
                        A.Address,
                        A.Latitude,
                        A.Longitude,
                        ({haversineFormula}) AS DistanceMiles,
                        D.Description AS DepartmentName,
                        A.Phone,
                        A.Email
                    FROM tblApplicants A
                    LEFT JOIN tblDepartment D ON A.idDepartment = D.Id
                    WHERE A.Latitude IS NOT NULL AND A.Longitude IS NOT NULL
                ");
                    }

                    if (idDepartment.HasValue && idDepartment.Value > 0)
                    {
                        queryBuilder.Append(" AND " + (entityType == "Staff" ? "S" : "A") + ".idDepartment = @idDepartment");
                        cmd.Parameters.AddWithValue("@idDepartment", idDepartment.Value);
                    }

                    queryBuilder.Append(" ORDER BY DistanceMiles;");
                    cmd.CommandText = queryBuilder.ToString();

                    // Función auxiliar por si el ACOS da error con valores cercanos a 1
                    cmd.CommandText = cmd.CommandText.Replace("pmin(1.0, pmax(-1.0, ", "").Replace("))", ")");

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            results.Add(new GeoResult
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                EntityType = dr["EntityType"].ToString(),
                                Name = dr["Name"].ToString(),
                                Address = dr["Address"].ToString(),
                                Latitude = Convert.ToDecimal(dr["Latitude"]),
                                Longitude = Convert.ToDecimal(dr["Longitude"]),
                                DistanceMiles = Convert.ToDouble(dr["DistanceMiles"]),
                                DepartmentName = dr["DepartmentName"].ToString(),
                                Phone = dr["Phone"].ToString(),
                                Email = dr["Email"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error");
            }
            return results;
        }

        // NOTA IMPORTANTE: La implementación de Haversine asume que tblApplicants también tendrá Latitude y Longitude.
        // Si no es así, la distancia será incorrecta o no se encontrarán registros.
        // Te recomiendo agregar [Latitude] [decimal](9, 6) NULL, [Longitude] [decimal](9, 6) NULL a tblApplicants.
    }
}