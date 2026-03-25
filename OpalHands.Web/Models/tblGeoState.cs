using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class tblGeoState
    {
        [Key]
        public int Id { get; set; }
        public string? Description { get; set; }
    }
}