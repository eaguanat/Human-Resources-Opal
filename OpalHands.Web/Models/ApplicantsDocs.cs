using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class ApplicantsDocs
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdDepartment { get; set; } // La llave que acabas de agregar

        [Required]
        public string Description { get; set; }
    }
}