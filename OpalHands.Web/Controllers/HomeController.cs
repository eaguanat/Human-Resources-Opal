using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpalHands.Web.Data;
using OpalHands.Web.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpalHands.Web.Controllers
{
    public class HomeController : Controller
    {
        // El "motor" que conecta este controlador con tu base de datos Azure
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        // Constructor: Aquí inyectamos el contexto de datos (Solución al error que tenías)
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // PÁGINA DE INICIO (HOME)
        public IActionResult Index()
        {
            return View();
        }

        // PÁGINA "ABOUT US" (Tu Cultura Organizacional)
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult SessionExpired()
        {
            return View();
        }



        // NUEVA PÁGINA "CONTACT US" (Extrae datos de tblCompany)
        public async Task<IActionResult> Contact()
        {
            try
            {
                var company = await _context.tblCompany.FirstOrDefaultAsync();

                if (company != null)
                {
                    var city = await _context.tblGeoCity.FirstOrDefaultAsync(c => c.Id == company.idGeoCity);
                    var state = await _context.tblGeoState.FirstOrDefaultAsync(s => s.Id == company.idGeoState);

                    ViewBag.CityName = city?.Description;
                    ViewBag.StateName = state?.Description;
                }

                return View(company ?? new tblCompany());
            }
            catch (Exception ex)
            {
                // Si falla la red o el host, lo mandamos a la salida elegante
                _logger.LogError(ex, "Error de conexión en la página de Contacto");
                return RedirectToAction("Index"); // O a tu nueva vista de Error de Conexión
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}