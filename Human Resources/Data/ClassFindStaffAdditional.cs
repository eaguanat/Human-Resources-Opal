using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace Human_Resources.Data
{
    public class ClassFindStaffAdditional
    {
        // METODO: Obtener Staff que debe documentos requeridos
        public DataTable ObtenerStaffConFaltantes(int idDepartment, string name, int idRegion, bool soloActivos, int idState, int idCity, string zip, int idLicense, int idPayroll, int idBank, int idMethod, int idInmigration)
        {
            // 1. EL FILTRO MAESTRO: Obtenemos el universo según criterios geográficos/laborales
            ClassStaff staffData = new ClassStaff();
            DataTable dtPersonalFiltrado = staffData.FiltrarPersonal(idDepartment, name, idRegion, soloActivos, idState, idCity, zip, idLicense, idPayroll, idBank, idMethod, idInmigration);

            if (dtPersonalFiltrado.Rows.Count == 0) return dtPersonalFiltrado;

            // Convertimos los IDs a una lista para el WHERE IN
            string idsCualificados = string.Join(",", dtPersonalFiltrado.AsEnumerable().Select(r => r["id"].ToString()));

            DataTable dtResultado = new DataTable();

            // 2. LA CIRUGÍA: Buscamos Staff que tenga al menos un documento 'Required' 
            // que NO exista en su historial de 'tblStaffDocsAdditional'
            string query = $@"
                           SELECT DISTINCT S.Id, S.LastName + ', ' + S.Name AS FullName
                           FROM tblStaff S
                           WHERE S.id IN ({idsCualificados})
                           AND EXISTS (
                           SELECT 1 FROM tblDocsAdditional DA 
                           WHERE DA.Required = 1 
                           AND NOT EXISTS (
                           SELECT 1 FROM tblStaffDocsAdditional SDA 
                           WHERE SDA.idStaff = S.id 
                           AND SDA.idDocsAdditional = DA.id ))
                           ORDER BY FullName";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dtResultado);
            }

            return dtResultado;
        }


        public DataTable ObtenerDetalleFaltantes(int idStaff)
        {
            DataTable dt = new DataTable();
            string query = @"
           SELECT Description, 'PENDING' as Status 
           FROM tblDocsAdditional 
           WHERE Required = 1 
           AND Id NOT IN (SELECT IdDocsAdditional FROM tblStaffDocsAdditional WHERE IdStaff = @idStaff)
           ORDER BY Description";

            using (SqlConnection conn = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                // Ahora sí coinciden: @idStaff en el Query y @idStaff en el parámetro
                cmd.Parameters.AddWithValue("@idStaff", idStaff);
                new SqlDataAdapter(cmd).Fill(dt);
            }
            return dt;
        }


        // REPORTE MASIVO: Obtiene todos los faltantes de todos los empleados que cumplen el filtro.
        public DataTable ObtenerDetalleFaltantesMasivo(int idDepartment, string name, int idRegion, bool soloActivos, int idState, int idCity, string zip, int idLicense, int idPayroll, int idBank, int idMethod, int idInmigration)
        {
            ClassStaff staffData = new ClassStaff();
            DataTable dtPersonalFiltrado = staffData.FiltrarPersonal(idDepartment, name, idRegion, soloActivos, idState, idCity, zip, idLicense, idPayroll, idBank, idMethod, idInmigration);

            if (dtPersonalFiltrado.Rows.Count == 0) return dtPersonalFiltrado;

            string idsCualificados = string.Join(",", dtPersonalFiltrado.AsEnumerable().Select(r => r["id"].ToString()));

            DataTable dtResultado = new DataTable();

            // Cruzamos el personal filtrado con los documentos requeridos que NO poseen
            string query = $@"
                           SELECT 
                           R.Description as RegionName,
                           S.LastName + ', ' + S.Name AS FullName,
                           DA.Description as DocName
                           FROM tblStaff S
                           INNER JOIN tblGeoRegion R ON S.idGeoRegion = R.id
                           CROSS JOIN tblDocsAdditional DA
                           WHERE S.id IN ({idsCualificados})
                           AND DA.Required = 1
                           AND NOT EXISTS (
                           SELECT 1 FROM tblStaffDocsAdditional SDA 
                           WHERE SDA.idStaff = S.id 
                           AND SDA.idDocsAdditional = DA.id )
                           ORDER BY RegionName, FullName, DocName";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dtResultado);
            }

            return dtResultado;
        }


        private DataTable EjecutarQuery(string query)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ClassConexion.CadenaConexion))
            {
                new SqlDataAdapter(query, conn).Fill(dt);
            }
            return dt;
        }

        public void ImprimirDocSilencioso(FlowDocument doc, string fullPath)
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            using (XpsDocument xpsDoc = new XpsDocument(fullPath, FileAccess.Write))
            {
                XpsDocument.CreateXpsDocumentWriter(xpsDoc).Write(((IDocumentPaginatorSource)doc).DocumentPaginator);
            }
        }




        // ------------C O M B O S ------------
        // --- MÉTODOS DE APOYO (COMBOS) ---
        public DataTable ObtenerDepartementos()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id, Description FROM tblDepartment ORDER BY Description ASC";
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerLicense(int idDepartment)
        {
            DataTable dt = new DataTable();
            // Filtramos tblGeoCity usando la columna IdGeoState
            string query = "SELECT Id, Description FROM tblDepLicense WHERE idDepartment = @idDepartment ORDER BY Description ASC";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@idDepartment", idDepartment);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerRegiones()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id, Description FROM tblGeoRegion ORDER BY Description ASC";
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }


        public DataTable ObtenerState()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id, Description FROM tblGeoState ORDER BY Description ASC";
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerCity(int idEstado)
        {
            DataTable dt = new DataTable();
            // Filtramos tblGeoCity usando la columna IdGeoState
            string query = "SELECT Id, Description FROM tblGeoCity WHERE IdGeoState = @idEstado ORDER BY Description ASC";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@idEstado", idEstado);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
        public DataTable ObtenerDocsInmigration()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id, Description FROM tblDocsInmigration ORDER BY Description ASC";
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerBancos()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id, Description FROM tblBanks ORDER BY Description ASC";
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }
        public DataTable ObtenerDocsPayroll()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id, Description FROM tblDocsPayroll ORDER BY Description ASC";
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerBanksMethod()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id, Description FROM tblBanksMethod ORDER BY Description ASC";
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }

    }
}