using Microsoft.AspNetCore.Mvc;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Controllers
{
    public class FeeTypeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public FeeTypeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<FeeType> feeTypes = _db.FeeTypes.ToList();
            return View(feeTypes);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(FeeType feeType)
        {
            if (!ModelState.IsValid)
                return View(feeType);

            try
            {
                await _db.FeeTypes.AddAsync(feeType);
                await _db.SaveChangesAsync();

                TempData["Message"] = "FeeType Created successfully!";
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Consider logging the error instead of writing to console in production
                // Example: _logger.LogError(ex, "Error creating class.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return View(feeType);
            }
        }
        public IActionResult Edit(Guid feeTypeId)
        {
            if (feeTypeId == Guid.Empty)
            {
                return NoContent();
            }
            FeeType feeType = _db.FeeTypes.FirstOrDefault(c => c.FeeTypeId == feeTypeId);
            if (feeType == null)
            {
                return NoContent();
            }
            return View(feeType);
        }
        [HttpPost]
        public async Task<IActionResult> EditAsync(FeeType feeType)
        {
            if (!ModelState.IsValid)
                return View(feeType);
            try
            {
                _db.FeeTypes.Update(feeType);
                await _db.SaveChangesAsync();

                TempData["Message"] = "FeeType Updated successfully!";
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return View(feeType);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAsync(Guid feeTypeId)
        {
            var findfeetType = await _db.FeeTypes.FindAsync(feeTypeId);
            if (findfeetType == null)
            {
                return NotFound();
            }
            FeeType feeType = _db.FeeTypes.FirstOrDefault(p => p.FeeTypeId == feeTypeId);
            return View(feeType);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAsync(FeeType feeType)
        {
            _db.FeeTypes.Remove(feeType);
            await _db.SaveChangesAsync();
            TempData["Message"] = "FeeType Deleted Successfully!";
            TempData["MessageType"] = "error";
            return RedirectToAction("Index");

        }

    }
}
