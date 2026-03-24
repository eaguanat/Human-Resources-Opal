using Microsoft.EntityFrameworkCore; // NUEVO: Necesario para usar SQL Server
using OpalHands.Web.Data;          // NUEVO: Para encontrar tu ApplicationDbContext

namespace OpalHands.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // --- CONFIGURACIÓN DE LA CONEXIÓN (NUEVO) ---
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // --------------------------------------------

            var app = builder.Build();

            // ... (el resto del código se queda igual)
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}