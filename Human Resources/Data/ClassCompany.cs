using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace Human_Resources.Data
{
    public class ClassCompany // Corregido el nombre de ClassCompay a ClassCompany
    {
        // --- PROPIEDADES ---
        public int Id { get; set; }
        public string Name { get; set; }
        public int? IdGeoRegion { get; set; }
        public string Address { get; set; }
        public int? IdGeoState { get; set; }
        public int? IdGeoCity { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public byte[] Logo { get; set; } // Cambiado de PictureBox a byte[] para la BD

        // --- MÉTODOS ---
        public bool ObtenerPorId(int idBusqueda)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    string query = "SELECT * FROM tblCompany WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idBusqueda);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        this.Id = Convert.ToInt32(dr["id"]);
                        this.Name = dr["Name"].ToString();
                        this.Address = dr["Address"].ToString();
                        this.IdGeoRegion = dr["IdGeoRegion"] != DBNull.Value ? Convert.ToInt32(dr["IdGeoRegion"]) : (int?)null;
                        this.IdGeoState = dr["idGeoState"] != DBNull.Value ? Convert.ToInt32(dr["idGeoState"]) : (int?)null;
                        this.IdGeoCity = dr["idGeoCity"] != DBNull.Value ? Convert.ToInt32(dr["idGeoCity"]) : (int?)null;
                        this.ZipCode = dr["ZipCode"].ToString();
                        this.Phone = dr["Phone"].ToString();
                        this.Email = dr["Email"].ToString();
                        this.Logo = dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : null;
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
                    // Corregido el ORDER BY para usar Name
                    string query = @"SELECT id, Name, Address, Phone
                                     FROM tblCompany 
                                     WHERE Name LIKE @f 
                                     ORDER BY Name";
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
                    // Corregido @ZipCod por @ZipCode
                    string query = @"INSERT INTO tblCompany (Name, Address, IdGeoRegion, idGeoState, idGeoCity, ZipCode, Phone, Email, Logo) 
                                     VALUES (@Name, @Address, @IdGeoRegion, @idGeoState, @idGeoCity, @ZipCode, @Phone, @Email, @Logo);
                                     SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SetParameters(cmd);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Insert): " + ex.Message,"Insertar"); return 0; }
        }

        public bool Actualizar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    // Corregido tblComapny por tblCompany
                    string query = @"UPDATE tblCompany SET Name=@Name, Address=@Address, IdGeoRegion=@IdGeoRegion, 
                                     idGeoState=@idGeoState, idGeoCity=@idGeoCity, ZipCode=@ZipCode, Phone=@Phone, 
                                     Email=@Email, Logo=@Logo 
                                     WHERE id=@id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    SetParameters(cmd);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Update): " + ex.Message,"Actualizar"); return false; }
        }

        public bool Eliminar(int idEliminar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM tblCompany WHERE id = @id", con);
                    cmd.Parameters.AddWithValue("@id", idEliminar);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Error (Delete): " + ex.Message); return false; }
        }

        private void SetParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Name", (object)Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object)Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdGeoRegion", (object)IdGeoRegion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoState", (object)IdGeoState ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idGeoCity", (object)IdGeoCity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ZipCode", (object)ZipCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);
            // lOGO
            SqlParameter paramLogo = new SqlParameter("@Logo", SqlDbType.VarBinary);
            paramLogo.Value = (object)Logo ?? DBNull.Value;
            cmd.Parameters.Add(paramLogo);
        }
 
    }
}