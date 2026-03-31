using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpalHands.Web.Data;
using OpalHands.Web.Models;
using OpalHands.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpalHands.Web.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService; // <--- Agregar esta línea

        public RegistrationController(ApplicationDbContext context, EmailService emailService) // <--- Agregar aquí también
        {
            _context = context;
            _emailService = emailService;
        }

        
        public async Task<IActionResult> Index()
        {
            return View(await _context.tblApplicants.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tblApplicants = await _context.tblApplicants
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tblApplicants == null) return NotFound();

            return View(tblApplicants);
        }

        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.tblDepartment, "Id", "Description");
            ViewBag.States = new SelectList(_context.tblGeoState, "Id", "Description");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(tblApplicants applicant)
        {
            // --- PASO B: LIMPIEZA DE HONOR (INTACTA) ---
            applicant.FirstName = CleanText(applicant.FirstName);
            applicant.LastName = CleanText(applicant.LastName);
            applicant.Address = CleanText(applicant.Address);
            applicant.ServiceZipCodes = CleanText(applicant.ServiceZipCodes);
            applicant.Observations = CleanText(applicant.Observations);

            if (applicant.Email != null) applicant.Email = applicant.Email.ToLower().Trim();

            // 1. VERIFICACIÓN DE SEGURIDAD (INTACTA)
            var exists = _context.tblApplicants.Any(a => a.Email == applicant.Email);

            if (exists)
            {
                ModelState.AddModelError("Email", "We are analyzing your application; we will contact you via email.");
                ViewBag.Departments = new SelectList(_context.tblDepartment, "Id", "Description");
                ViewBag.States = new SelectList(_context.tblGeoState, "Id", "Description");
                return View(applicant);
            }

            if (ModelState.IsValid)
            {
                // 2. GUARDADO EN AZURE (INTACTO)
                _context.Add(applicant);
                await _context.SaveChangesAsync();

                // 3. --- INICIO DE LA CIRUGÍA DEL CARTERO ---
                try
                {
                    // Combinamos nombre y apellido para el PDF
                    string fullNombre = $"{applicant.FirstName} {applicant.LastName}";

                    // Llamamos al servicio (Correo + PDF)
                    await _emailService.SendWelcomeEmailWithPdfAsync(
                        applicant.Email ?? "",
                        fullNombre,
                        applicant.idDepartment ?? 0
                    );
                }
                catch (Exception ex)
                {
                    // Si el correo falla, registramos el error en la consola de VS
                    // Pero permitimos que el usuario vea la pantalla de éxito porque ya se guardó.
                    System.Diagnostics.Debug.WriteLine("Error enviando correo: " + ex.Message);
                }
                // --- FIN DE LA CIRUGÍA ---

                // 4. DATOS PARA LA VISTA DE ÉXITO (INTACTO)
                var company = await _context.tblCompany.FirstOrDefaultAsync();
                TempData["ApplicantName"] = applicant.FirstName;
                TempData["ApplicantEmail"] = applicant.Email;
                TempData["CompanyId"] = company?.id;

                return RedirectToAction(nameof(Success));
            }

            ViewBag.Departments = new SelectList(_context.tblDepartment, "Id", "Description");
            ViewBag.States = new SelectList(_context.tblGeoState, "Id", "Description");
            return View(applicant);
        }

        // --- ACCIÓN DE BIENVENIDA / SUCCESS PAGE ---
        public IActionResult Success()
        {
            // Recuperamos los datos ligeros para la vista
            ViewBag.Name = TempData["ApplicantName"];
            ViewBag.ApplicantEmail = TempData["ApplicantEmail"];
            ViewBag.CompanyId = TempData["CompanyId"];

            return View();
        }

        // --- MOTOR DE LOGO (NUEVO MÉTODO QUIRÚRGICO) ---
        [HttpGet]
        public async Task<IActionResult> GetCompanyLogo(int id)
        {
            var company = await _context.tblCompany.FindAsync(id);
            if (company?.Logo != null)
            {
                // Retorna los bytes directamente como imagen
                return File(company.Logo, "image/png");
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult CheckEmail(string email)
        {
            var exists = _context.tblApplicants.Any(a => a.Email == email);
            return Json(new { isDuplicate = exists });
        }

        [HttpGet]
        public IActionResult GetCities(int stateId)
        {
            var cities = _context.tblGeoCity
                .Where(c => c.IdGeoState == stateId)
                .Select(c => new { id = c.Id, description = c.Description })
                .ToList();

            return Json(cities);
        }

        private string CleanText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            string stFormD = text.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char ch in stFormD)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToUpper().Trim();
        }

        private bool tblApplicantsExists(int id)
        {
            return _context.tblApplicants.Any(e => e.Id == id);
        }
    }
}