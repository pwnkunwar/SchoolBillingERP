using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Controllers
{
    [Authorize]

    public class FiscalYearController : Controller
    {
        private readonly ApplicationDbContext _db;
        public FiscalYearController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<FiscalYear> fiscalYears = _db.FiscalYears.ToList();
            return View(fiscalYears);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(FiscalYear fiscalYear)
        {
            if (!ModelState.IsValid)
                return View(fiscalYear);

            try
            {
                await _db.FiscalYears.AddAsync(fiscalYear);
                await _db.SaveChangesAsync();

                TempData["Message"] = "Fiscal Year Created successfully!";
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Consider logging the error instead of writing to console in production
                // Example: _logger.LogError(ex, "Error creating class.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return View(fiscalYear);
            }
        }
        public IActionResult Edit(Guid fiscalYearId)
        {
            if (fiscalYearId == Guid.Empty)
            {
                return NoContent();
            }
            FiscalYear fiscalYear = _db.FiscalYears.FirstOrDefault(c => c.Id == fiscalYearId);
            if (fiscalYear == null)
            {
                return NoContent();
            }
            return View(fiscalYear);
        }
        [HttpPost]
        public async Task<IActionResult> EditAsync(FiscalYear fiscalYear)
        {
            if (!ModelState.IsValid)
                return View(fiscalYear);
            try
            {
                _db.FiscalYears.Update(fiscalYear);
                await _db.SaveChangesAsync();

                TempData["Message"] = "Fiscal Year Updated successfully!";
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return View(fiscalYear);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAsync(Guid fiscalYearId)
        {
            var findfeetType = await _db.FiscalYears.FindAsync(fiscalYearId);
            if (findfeetType == null)
            {
                return NotFound();
            }
            FiscalYear fiscalYear = _db.FiscalYears.FirstOrDefault(p => p.Id == fiscalYearId);
            return View(fiscalYear);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAsync(FiscalYear fiscalYear)
        {
            _db.FiscalYears.Remove(fiscalYear);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Fiscal Year Deleted Successfully!";
            TempData["MessageType"] = "error";
            return RedirectToAction("Index");

        }

    }
}

