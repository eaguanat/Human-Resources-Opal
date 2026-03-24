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
            return View();
        }

        // POST: Registration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Applicants applicant)
        {
            // 1. EL FILTRO DE SEGURIDAD: ¿Ya existe el correo?
            var exists = _context.tblApplicants.Any(a => a.Email == applicant.Email);

            if (exists)
            {
                // Si ya existe, enviamos el mensaje que tú definiste
                ModelState.AddModelError("Email", "Estamos analizando su propuesta, lo contactaremos a través de su correo electrónico.");
                return View(applicant);
            }

            if (ModelState.IsValid)
            {
                // 2. AUTOMATIZACIÓN: Campos que el usuario no llena
                applicant.DateCreated = DateTime.Now;
                applicant.Status = 1; // 1 = Nuevo / Pendiente

                // 3. GUARDADO ATÓMICO: Solo si pasó todas las reglas
                _context.Add(applicant);
                await _context.SaveChangesAsync();

                // 4. EL ENTREGABLE: Redirigir a la lista de documentos según el departamento
                return RedirectToAction("Requirements", new { id = applicant.idDepartment });
            }

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
    }
}
