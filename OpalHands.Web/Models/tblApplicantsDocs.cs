using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class tblApplicantsDocs
    {
        [Key]
        public int Id { get; set; }

        public int? idDepartment { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}