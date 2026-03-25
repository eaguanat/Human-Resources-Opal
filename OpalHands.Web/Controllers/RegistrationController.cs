using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpalHands.Web.Data;
using OpalHands.Web.Models;

namespace OpalHands.Web.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Registration
        public async Task<IActionResult> Index()
        {
            return View(await _context.tblApplicants.ToListAsync());
        }

        // GET: Registration/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicants = await _context.tblApplicants
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicants == null)
            {
                return NotFound();
            }

            return View(applicants);
        }

        // GET: Registration/Create
        public IActionResult Create()
        {
            // Cargamos los SelectLists para que nazcan con la página
            ViewBag.Departments = new SelectList(_context.tblDepartment, "Id", "Description");
            ViewBag.States = new SelectList(_context.tblGeoState, "Id", "Description");
            return View();
        }

        // POST: Registration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(tblApplicants applicant)
        {
            // --- PASO B: LIMPIEZA DE HONOR (Mayúsculas y sin tildes) ---
            applicant.FirstName = CleanText(applicant.FirstName);
            applicant.LastName = CleanText(applicant.LastName);
            applicant.Address = CleanText(applicant.Address);
            // El Email lo pasamos a minúsculas por estándar de sistemas
            if (applicant.Email != null) applicant.Email = applicant.Email.ToLower().Trim();

            // 1. VERIFICACIÓN DE SEGURIDAD
            var exists = _context.tblApplicants.Any(a => a.Email == applicant.Email);

            if (exists)
            {
                ModelState.AddModelError("Email", "We are analyzing your application; we will contact you via email. / Estamos analizando su propuesta; lo contactaremos a través de su correo electrónico.");

                // Recargamos las listas por si el usuario tiene que corregir
                ViewBag.Departments = new SelectList(_context.tblDepartment, "Id", "Description");
                ViewBag.States = new SelectList(_context.tblGeoState, "Id", "Description");
                return View(applicant);
            }

            if (ModelState.IsValid)
            {
                // 2. AUTOMATIZACIÓN DE CAMPOS
                applicant.DateCreated = DateTime.Now;
                applicant.Status = 1; // 1 = Nuevo

                // 3. GUARDADO EN AZURE
                _context.Add(applicant);
                await _context.SaveChangesAsync();

                // 4. REDIRECCIÓN (A la siguiente etapa)
                return RedirectToAction("Requirements", new { id = applicant.idDepartment });
            }

            // Si algo falla, recargamos las listas para no romper la vista
            ViewBag.Departments = new SelectList(_context.tblDepartment, "Id", "Description");
            ViewBag.States = new SelectList(_context.tblGeoState, "Id", "Description");
            return View(applicant);
        }

        // GET: Registration/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicants = await _context.tblApplicants.FindAsync(id);
            if (applicants == null)
            {
                return NotFound();
            }
            return View(applicants);
        }

        // POST: Registration/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Password,FirstName,LastName,idDepartment,idGeoState,idGeoCity,Address,ZipCode,Phone,Status,DateCreated,LastLogin")] Applicants applicants)
        {
            if (id != applicants.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicants);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicantsExists(applicants.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(applicants);
        }

        // GET: Registration/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicants = await _context.tblApplicants
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicants == null)
            {
                return NotFound();
            }

            return View(applicants);
        }

        // POST: Registration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applicants = await _context.tblApplicants.FindAsync(id);
            if (applicants != null)
            {
                _context.tblApplicants.Remove(applicants);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicantsExists(int id)
        {
            return _context.tblApplicants.Any(e => e.Id == id);
        }

        // --- CÓDIGO DE VERIFICACIÓN EN VIVO / LIVE VERIFICATION CODE ---

        // Esta función escanea la base de datos de Azure buscando el Email.
        // Al ser [HttpGet], la web puede consultarla sin recargar la página.
        [HttpGet]
        public IActionResult CheckEmail(string email)
        {
            // Buscamos si ya existe en tblApplicants
            var exists = _context.tblApplicants.Any(a => a.Email == email);

            // Devolvemos la respuesta como un dato simple (isDuplicate: true o false)
            return Json(new { isDuplicate = exists });
        }

        [HttpGet]
        public IActionResult GetCities(int stateId)
        {
            // Buscamos solo las ciudades que pertenecen al estado seleccionado
            var cities = _context.tblGeoCity
                .Where(c => c.IdGeoState == stateId)
                .Select(c => new { id = c.Id, description = c.Description })
                .ToList();

            return Json(cities);
        }

        // El "?" en (string? text) le dice a C# que el texto puede ser nulo
        private string CleanText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // 1. Normalizar para separar letras de tildes
            string stFormD = text.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (char ch in stFormD)
            {
                // Solo conservamos caracteres que no sean marcas de acento (tildes)
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            // 2. Retornar en Mayúsculas y limpio
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToUpper().Trim();
        }
    }
}
