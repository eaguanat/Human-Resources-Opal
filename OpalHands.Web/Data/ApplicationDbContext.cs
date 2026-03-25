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
        public DbSet<tblApplicants> tblApplicants { get; set; }
        public DbSet<tblApplicantsDocs> tblApplicantsDocs { get; set; }
        public DbSet<tblDepartment> tblDepartment { get; set; }
        public DbSet<tblGeoState> tblGeoState { get; set; }
        public DbSet<tblGeoCity> tblGeoCity { get; set; }
    }
}