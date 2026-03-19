using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls; // Para Grid y TextBlock
using System.Windows.Documents;
using System.Windows.Media;   // Para Brushes y FontFamily
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;



namespace Human_Resources.Data
{
    public class ClassFindStaff
    {
        // LOCALIZANDO EL STAFF CON LA DOCUMENTACION VENCIDA
        public DataTable ObtenerStaffConVencimientos(int idDepartment, string name, int idRegion, bool soloActivos, DateTime? desde, DateTime? hasta, int idState, int idCity, string zip, int idLicense, int idPayroll, int idBank, int idMethod, int idInmigration)
        {
            ClassStaff staffData = new ClassStaff();
            DataTable dtPersonalFiltrado = staffData.FiltrarPersonal(idDepartment, name, idRegion, soloActivos, idState, idCity, zip, idLicense, idPayroll, idBank, idMethod, idInmigration);

            if (dtPersonalFiltrado.Rows.Count == 0) return dtPersonalFiltrado;

            DataTable dtResultado = new DataTable();

            // Extraemos los IDs para la subconsulta
            string idsCualificados = string.Join(",", dtPersonalFiltrado.AsEnumerable().Select(r => r["id"].ToString()));

            // Agregamos DISTINCT para que el Staff no se repita por cada documento
            string query = $@"
                           WITH UltimosDocumentos AS (
                           SELECT idStaff, idDocsRequired, DateDoc,
                           ROW_NUMBER() OVER (PARTITION BY idStaff, idDocsRequired ORDER BY DateDoc DESC, id DESC) as Ranking
                           FROM tblStaffDocsRequired
                           WHERE idStaff IN ({idsCualificados})  )       
                           SELECT DISTINCT S.id, S.LastName + ', ' + S.Name AS FullName
                           FROM tblStaff S
                           INNER JOIN tblDocsRequired D ON D.idDepartment = S.idDepartment
                           LEFT JOIN UltimosDocumentos UD ON S.id = UD.idStaff AND D.id = UD.idDocsRequired AND UD.Ranking = 1
                           WHERE S.id IN ({idsCualificados})
                           AND (UD.DateDoc IS NULL OR (UD.DateDoc BETWEEN @desde AND @hasta))
                           ORDER BY FullName";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@desde", desde ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@hasta", hasta ?? (object)DBNull.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtResultado);
            }

            return dtResultado;
        }


        // MÉTODO DE REPORTE MASIVO: También actualizado con la lógica de "Última Fecha"
        public DataTable ObtenerDetalleVencidosPorStaff(int idStaff, int idDepartment, DateTime? desde, DateTime? hasta)
        {
            DataTable dt = new DataTable();
            string query = @"
                           WITH UltimosDocumentos AS (
                           SELECT idStaff, idDocsRequired, DateDoc,
                           ROW_NUMBER() OVER (PARTITION BY idStaff, idDocsRequired ORDER BY DateDoc DESC, id DESC) as Ranking
                           FROM tblStaffDocsRequired
                           WHERE idStaff = @idStaff  )
                           SELECT D.Description, UD.DateDoc
                           FROM tblDocsRequired D
                           LEFT JOIN UltimosDocumentos UD ON D.id = UD.idDocsRequired AND UD.Ranking = 1
                           WHERE D.idDepartment = @idDepartment
                           AND (UD.DateDoc IS NULL OR (UD.DateDoc BETWEEN @desde AND @hasta))
                           ORDER BY D.Description ASC"; 

             using (SqlConnection conn = new SqlConnection(ClassConexion.CadenaConexion))
                {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idStaff", idStaff);
                cmd.Parameters.AddWithValue("@idDepartment", idDepartment);
                cmd.Parameters.AddWithValue("@desde", desde ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@hasta", hasta ?? (object)DBNull.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }



        // CARGANDO LOS COMBOS

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

        public DataTable ObtenerDetalleVencidosMasivo(int idDepartment, string name, int idRegion, bool soloActivos, DateTime desde, DateTime hasta, int idState, int idCity, string zip, int idLicense, int idPayroll, int idBank, int idMethod, int idInmigration)
        {
            DataTable dt = new DataTable();

            // Construimos el query base con los JOINs necesarios para el reporte
            string query = @"
        SELECT 
            R.Description as RegionName,
            S.LastName + ', ' + S.Name AS FullName,
            D.Description, 
            SD.DateDoc
        FROM tblStaff S
        INNER JOIN tblGeoRegion R ON S.idGeoRegion = R.id
        CROSS JOIN tblDocsRequired D
        LEFT JOIN tblStaffDocsRequired SD ON S.id = SD.idStaff AND D.id = SD.idDocsRequired
        WHERE 1=1";

            // Filtros dinámicos (Deben ser idénticos a tu Método 1)
            query += " AND S.Active = " + (soloActivos ? "1" : "0");
            if (!string.IsNullOrEmpty(name)) query += " AND (S.Name LIKE @name OR S.LastName LIKE @name)";
            if (idRegion > 0) query += " AND S.idGeoRegion = " + idRegion;
            if (idState > 0) query += " AND S.idGeoState = " + idState;
            if (idCity > 0) query += " AND S.idGeoCity = " + idCity;
            if (!string.IsNullOrEmpty(zip)) query += " AND S.ZipCod = @zip";
            if (idPayroll > 0) query += " AND S.idDocsPayroll = " + idPayroll;
            if (idBank > 0) query += " AND S.idBanks = " + idBank;
            if (idMethod > 0) query += " AND S.idBanksMethod = " + idMethod;
            if (idInmigration > 0) query += " AND S.idDocsInmigration = " + idInmigration;

            // Lógica de fechas (Aquí es donde SQL busca las variables)
            query += " AND (SD.DateDoc IS NULL OR (SD.DateDoc BETWEEN @desde AND @hasta))";
            query += " ORDER BY R.Description, FullName";

            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, con);

                // IMPORTANTÍSIMO: Declarar los parámetros que SQL espera
                if (!string.IsNullOrEmpty(name)) cmd.Parameters.AddWithValue("@name", "%" + name + "%");
                if (!string.IsNullOrEmpty(zip)) cmd.Parameters.AddWithValue("@zip", zip);

                // Aquí se soluciona tu error:
                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public void ImprimirDocSilencioso(FlowDocument doc, string fullPath)
        {
            fullPath = Path.ChangeExtension(fullPath, ".xps");

            try
            {
                if (File.Exists(fullPath)) File.Delete(fullPath);

                using (XpsDocument xpsDoc = new XpsDocument(fullPath, FileAccess.Write))
                {
                    XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);

                    // Convertimos el FlowDocument a un paginador
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;

                    // Forzamos tamaño carta
                    paginator.PageSize = new Size(816, 1056);

                    writer.Write(paginator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Technical error creating XPS: " + ex.Message);
            }
        }

    }
}