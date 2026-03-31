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

        // --- EL CARTERO ---
        public string? MailServer { get; set; }
        public int? MailPort { get; set; }
        public string? MailUsername { get; set; }
        public string? MailPassword { get; set; }
        public bool? EnableSSL { get; set; }
        public string? SenderName { get; set; }
    }
}