using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class tblDepartment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
    }
}