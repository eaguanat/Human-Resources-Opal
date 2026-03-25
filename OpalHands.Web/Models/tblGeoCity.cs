using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class tblGeoCity
    {
        [Key]
        public int Id { get; set; }
        public int? IdGeoState { get; set; } // Esta es la llave que une con el estado
        public string? Description { get; set; }
    }
}