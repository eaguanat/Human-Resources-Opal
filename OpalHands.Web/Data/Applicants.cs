using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class Applicants
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int? idDepartment { get; set; }
        public int? idGeoState { get; set; }
        public int? idGeoCity { get; set; }
        public string? Address { get; set; }
        public string? ZipCode { get; set; }
        public string? Phone { get; set; }
        public int? Status { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}