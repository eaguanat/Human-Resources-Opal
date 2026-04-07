using System;
using System.ComponentModel.DataAnnotations;

namespace OpalHands.Web.Models
{
    public class tblApplicants
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public int? idDepartment { get; set; }
        public int? idGeoState { get; set; }
        public int? idGeoCity { get; set; }

        public string? Address { get; set; }
        public string? ZipCode { get; set; }
        public string? Phone { get; set; }

        public int? Status { get; set; }
        public DateTime? DateCreated { get; set; }
  
        public string? ServiceZipCodes { get; set; }
        public string? Observations { get; set; }
    }
}