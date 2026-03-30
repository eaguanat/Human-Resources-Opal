using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class tblCompany
    {
        [Key]
        public int id { get; set; }

        public int? idGeoRegion { get; set; }

        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Address { get; set; }

        public int? idGeoState { get; set; }

        public int? idGeoCity { get; set; }

        [StringLength(5)]
        public string? ZipCode { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }

        // Aquí guardamos el Logo que ya tienes en la base de datos
        public byte[]? Logo { get; set; }
    }
}