using OpalHands.Web.Services;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpalHands.Web.Data;

namespace OpalHands.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configuración de Licencia PDF
            QuestPDF.Settings.License = LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            // 1. Inyección de Servicios
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddControllersWithViews();

            // --- 2. CONFIGURACIÓN MANUAL DE BASE DE DATOS (BYPASS DEFINITIVO) ---
            string connectionString;

            if (builder.Environment.IsDevelopment())
            {
                // CONEXIÓN LOCAL: Se activa con el perfil "Local-Debug"
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=bdHomeCare;Trusted_Connection=True;MultipleActiveResultSets=true";
            }
            else
            {
                // CONEXIÓN AZURE: Se activa con el perfil "Azure-Cloud" (Producción)
                // Usamos los datos exactos de su portal de Azure
                connectionString = "Server=tcp:opal-hands-db-server.database.windows.net,1433;Initial Catalog=db_opal_hands;Persist Security Info=False;User ID=admin_opal;Password=11e357a403T;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions => {
                    // Resiliencia para Azure: Reintentos en caso de micro-cortes
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));
            // -----------------------------------------------------------------------

            var app = builder.Build();

            // 3. Configuración del Pipeline de HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                // Muestra errores detallados en modo Local-Debug
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapStaticAssets();

            // Ruta predeterminada
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}