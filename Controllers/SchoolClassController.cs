using Microsoft.AspNetCore.Mvc;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Controllers
{
    public class SchoolClassController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SchoolClassController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<SchoolClass> schoolClasses = _db.SchoolClasses.ToList();
            return View(schoolClasses);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(SchoolClass schoolClass)
        {
            if (!ModelState.IsValid)
                return View(schoolClass);

            try
            {
                await _db.SchoolClasses.AddAsync(schoolClass);
                await _db.SaveChangesAsync();

                TempData["Message"] = "Class created successfully!";
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Consider logging the error instead of writing to console in production
                // Example: _logger.LogError(ex, "Error creating class.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return View(schoolClass);
            }
        }
        public IActionResult Edit(Guid classId)
        {
            if(classId == Guid.Empty)
            {
                return NoContent();
            }
            SchoolClass schoolClass = _db.SchoolClasses.FirstOrDefault(c => c.ClassId == classId);
            if (schoolClass == null)
            {
                return NoContent();
            }
            return View(schoolClass);
        }
        [HttpPost]
        public async Task<IActionResult> EditAsync(SchoolClass schoolClass)
        {
            if (!ModelState.IsValid)
                return View(schoolClass);
            try
            {
                _db.SchoolClasses.Update(schoolClass);
                await _db.SaveChangesAsync();

                TempData["Message"] = "Class Updated successfully!";
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return View(schoolClass);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAsync(Guid classId)
        {
            var findInstallmentType = await _db.SchoolClasses.FindAsync(classId);
            if (findInstallmentType == null)
            {
                return NotFound();
            }
            SchoolClass schoolClass =  _db.SchoolClasses.FirstOrDefault(p => p.ClassId == classId);
            return View(schoolClass);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAsync(SchoolClass schoolClass)
        {
            _db.SchoolClasses.Remove(schoolClass);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Class Deleted Successfully!";
            TempData["MessageType"] = "error";
            return RedirectToAction("Index");

        }

    }
}
