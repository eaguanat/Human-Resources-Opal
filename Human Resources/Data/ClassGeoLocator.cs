using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Human_Resources.Data
{
    // Modelo para los resultados de la búsqueda geográfica
    public class GeoResult
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        // --- CORRECCIÓN SENSEI: Guardamos los IDs como enteros para las búsquedas ---
        public int IdGeoCity { get; set; }
        public int IdGeoState { get; set; }
        public string ZipCode { get; set; }
        // -------------------------------------------------------------------------

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double DistanceMiles { get; set; }
        public string DepartmentName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class ClassGeoLocator
    {
        private const double EarthRadiusMiles = 3958.8;

        public List<GeoResult> GetNearestEntities(string entityType, int? idDepartment, decimal centerLat, decimal centerLng, int topX, bool activeOnly)
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
                    string haversineFormula = $@"{EarthRadiusMiles} * ACOS(
                        COS(@radCenterLat) * COS(Latitude * {degToRad}) *
                        COS(Longitude * {degToRad} - @radCenterLng) +
                        SIN(@radCenterLat) * SIN(Latitude * {degToRad})
                    )";

                    cmd.Parameters.AddWithValue("@radCenterLat", (double)centerLat * (Math.PI / 180.0));
                    cmd.Parameters.AddWithValue("@radCenterLng", (double)centerLng * (Math.PI / 180.0));
                    cmd.Parameters.AddWithValue("@topX", topX);
                    cmd.Parameters.AddWithValue("@active", activeOnly);

                    if (entityType == "Staff")
                    {
                        // SENSEI: Usamos "AS ZipCode" para unificar el nombre de la columna
                        queryBuilder.Append($@"
                            SELECT TOP (@topX)
                                S.Id, 'Staff' AS EntityType,
                                ISNULL(S.Name, '') + ' ' + ISNULL(S.LastName, '') AS Name,
                                S.Address, S.idGeoCity, S.idGeoState, S.ZipCod AS ZipCode, 
                                S.Latitude, S.Longitude,
                                ({haversineFormula}) AS DistanceMiles,
                                D.Description AS DepartmentName, S.Phone, S.Email
                            FROM tblStaff S
                            LEFT JOIN tblDepartment D ON S.idDepartment = D.Id
                            WHERE S.Latitude IS NOT NULL AND S.Longitude IS NOT NULL
                            AND S.Active = @active");
                    }
                    else if (entityType == "Applicant")
                    {
                        queryBuilder.Append($@"
                            SELECT TOP (@topX)
                                A.Id, 'Applicant' AS EntityType,
                                ISNULL(A.FirstName, '') + ' ' + ISNULL(A.LastName, '') AS Name,
                                A.Address, A.idGeoCity, A.idGeoState, A.ZipCode, 
                                A.Latitude, A.Longitude,
                                ({haversineFormula}) AS DistanceMiles,
                                D.Description AS DepartmentName, A.Phone, A.Email
                            FROM tblApplicants A
                            LEFT JOIN tblDepartment D ON A.idDepartment = D.Id
                            WHERE A.Latitude IS NOT NULL AND A.Longitude IS NOT NULL");
                    }

                    if (idDepartment.HasValue && idDepartment.Value > 0)
                    {
                        queryBuilder.Append(" AND " + (entityType == "Staff" ? "S" : "A") + ".idDepartment = @idDepartment");
                        cmd.Parameters.AddWithValue("@idDepartment", idDepartment.Value);
                    }

                    queryBuilder.Append(" ORDER BY DistanceMiles;");
                    cmd.CommandText = queryBuilder.ToString();

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
                                IdGeoCity = dr["idGeoCity"] != DBNull.Value ? Convert.ToInt32(dr["idGeoCity"]) : 0,
                                IdGeoState = dr["idGeoState"] != DBNull.Value ? Convert.ToInt32(dr["idGeoState"]) : 0,

                                // Como usamos el ALIAS en el SQL, aquí siempre se llama "ZipCode"
                                ZipCode = dr["ZipCode"] != DBNull.Value ? dr["ZipCode"].ToString() : "",

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
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Database Error"); }
            return results;
        }
    }
}