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

        // NUEVA PÁGINA "CONTACT US" (Extrae datos de tblCompany)
        public async Task<IActionResult> Contact()
        {
            // 1. Obtenemos los datos base de la compañía
            var company = await _context.tblCompany.FirstOrDefaultAsync();

            if (company != null)
            {
                // 2. Buscamos el nombre de la Ciudad usando el ID de la compañía
                var city = await _context.tblGeoCity
                    .FirstOrDefaultAsync(c => c.Id == company.idGeoCity);

                // 3. Buscamos el nombre del Estado usando el ID de la compañía
                var state = await _context.tblGeoState
                    .FirstOrDefaultAsync(s => s.Id == company.idGeoState);

                // 4. Guardamos los nombres en el ViewBag para la vista
                ViewBag.CityName = city?.Description;
                
                ViewBag.StateName = state?.Description;
            }

            return View(company ?? new tblCompany());
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