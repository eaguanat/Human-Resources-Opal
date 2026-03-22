using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Controls;

namespace Human_Resources.Data
{
    public class ClassStaffDocsRequired
    {
        // Propiedades para la tabla tblStaffDocsRequired
        public int Id { get; set; }
        public int IdStaff { get; set; }
        public int IdDocsRequired { get; set; }
        public DateTime? DateDoc { get; set; }

        // Listar Staff para el DataGrid inicial
        public DataTable ListarStaff(string filtro, int idDept)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = @"SELECT Id, Name, LastName FROM tblStaff 
                             WHERE idDepartment = @idDept 
                             AND (Name LIKE @f OR LastName LIKE @f)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@idDept", idDept);
                cmd.Parameters.AddWithValue("@f", "%" + filtro + "%");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // NUEVO: Obtener las Licencias para la sección POSITION del reporte
        public DataTable ObtenerLicenciasStaff(int idStaff)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = @"SELECT dl.Description, sl.LicenseNumber 
                             FROM tblStaffLicense sl
                             INNER JOIN tblDepLicense dl ON sl.idDepLicense = dl.id
                             WHERE sl.idStaff = @idStaff";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@idStaff", idStaff);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Obtener los documentos con su ULTIMA fecha registrada
        public DataTable ListarDocsPorPersona(int idStaff, int idSection)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = @"
                    SELECT R.id AS IdDocsRequired, R.Description, 
                           (SELECT TOP 1 id FROM tblStaffDocsRequired 
                            WHERE idStaff = @idStaff AND idDocsRequired = R.id 
                            ORDER BY DateDoc DESC) as IdRegistro,
                           (SELECT TOP 1 DateDoc FROM tblStaffDocsRequired 
                            WHERE idStaff = @idStaff AND idDocsRequired = R.id 
                            ORDER BY DateDoc DESC) as LastDate
                    FROM tblDocsRequired R
                    WHERE R.idDepartment = (SELECT idDepartment FROM tblStaff WHERE id = @idStaff)
                    AND R.idSection = @idSection 
                    AND R.Required = 1
                    ORDER BY R.Description ASC";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@idStaff", idStaff);
                cmd.Parameters.AddWithValue("@idSection", idSection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public bool Insertar()
        {
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = "INSERT INTO tblStaffDocsRequired (idStaff, idDocsRequired, DateDoc, DateLog) " +
                             "VALUES (@idS, @idD, @date, GETDATE())";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@idS", IdStaff);
                cmd.Parameters.AddWithValue("@idD", IdDocsRequired);
                cmd.Parameters.AddWithValue("@date", (object)DateDoc ?? DBNull.Value);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Actualizar()
        {
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = "UPDATE tblStaffDocsRequired SET DateDoc = @date, DateLog = GETDATE() WHERE id = @id";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@date", (object)DateDoc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", Id);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public DataTable ObtenerReporteTop4(int idStaff, int idSection)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = @"
            SELECT 
                Description,
                MAX(CASE WHEN RowNum = 4 THEN DateDoc END) as Initial,
                MAX(CASE WHEN RowNum = 3 THEN DateDoc END) as Annual1,
                MAX(CASE WHEN RowNum = 2 THEN DateDoc END) as Annual2,
                MAX(CASE WHEN RowNum = 1 THEN DateDoc END) as Annual3
            FROM (
                SELECT 
                    dr.Description,
                    sdr.DateDoc,
                    ROW_NUMBER() OVER (PARTITION BY dr.id ORDER BY sdr.DateDoc DESC) as RowNum
                FROM tblDocsRequired dr
                LEFT JOIN tblStaffDocsRequired sdr ON dr.id = sdr.idDocsRequired AND sdr.idStaff = @idS
                WHERE dr.idSection = @idSec
                AND dr.Required = 1
            ) AS SourceTable
            WHERE RowNum <= 4
            GROUP BY Description";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@idS", idStaff);
                cmd.Parameters.AddWithValue("@idSec", idSection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public DataTable ObtenerDataReporte(int idStaff, int idSection)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
            {
                string sql = @"
            SELECT 
                Description,
                MAX(CASE WHEN PosInversa = 1 THEN DateDoc END) AS Initial,
                MAX(CASE WHEN PosInversa = 2 THEN DateDoc END) AS Annual1,
                MAX(CASE WHEN PosInversa = 3 THEN DateDoc END) AS Annual2,
                MAX(CASE WHEN PosInversa = 4 THEN DateDoc END) AS Annual3
            FROM (
                SELECT 
                    dr.Description, 
                    sdr.DateDoc,
                    ROW_NUMBER() OVER(PARTITION BY dr.id ORDER BY sdr.DateDoc DESC) as PosReciente,
                    ROW_NUMBER() OVER(PARTITION BY dr.id ORDER BY sdr.DateDoc ASC) as PosInversa
                FROM tblDocsRequired dr
                LEFT JOIN tblStaffDocsRequired sdr ON dr.id = sdr.idDocsRequired AND sdr.idStaff = @ids
                WHERE dr.idSection = @idSec
                AND dr.Required = 1
            ) AS Fuente
            WHERE PosReciente <= 4
            GROUP BY Description";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ids", idStaff);
                cmd.Parameters.AddWithValue("@idSec", idSection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
    }
}