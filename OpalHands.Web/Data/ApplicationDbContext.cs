using Microsoft.EntityFrameworkCore;
using OpalHands.Web.Models;

namespace OpalHands.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Estas son las "ventanas" a tus tablas de SQL
        public DbSet<Department> tblDepartment { get; set; }
        public DbSet<ApplicantsDocs> tblApplicantsDocs { get; set; }
        public DbSet<Applicants> tblApplicants { get; set; }
    }
}