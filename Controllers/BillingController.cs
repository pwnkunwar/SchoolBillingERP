using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Controllers
{
   // [Authorize]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BillingController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddStudentFee()
        {
            List<SchoolClass> schoolClass = _db.SchoolClasses.ToList();
            var model = new StudentFeeViewModel
            {
                /*Students = _db.Students.Select(c => new SelectListItem
                {
                    Value = c.StudentId.ToString(),
                    Text = c.FullName
                }).ToList(),*/
                FiscalYear = _db.FiscalYears.Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name
                }).ToList(),

                FeeTypes = _db.FeeTypes.Select(c => new SelectListItem
                {
                    Value = c.FeeTypeId.ToString(),
                    Text = c.Name
                }).ToList(),

                Class = schoolClass.Select(i => new SelectListItem
                {
                    Value = i.ClassId.ToString(),
                    Text = i.ClassName
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> GetStudentsByClass(Guid classId)
        {
            if(classId == Guid.Empty)
            {
                return NoContent();
            }
            try
            {
                List<Student> students = await _db.Students
                    .Where(c => c.ClassId == classId)
                    .ToListAsync();
                return Json(students);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(Index);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddStudentFee(StudentFeeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                foreach(var fee in model.FeeTypeSelections)
                {
                    var studentFee = new StudentFee
                    {
                        StudentId = model.StudentId,
                        FeeTypeId = fee.FeeTypeId,
                        Amount = fee.Amount,
                        PaymentDate = model.PaymentDate,
                        FeeStatus = model.FeeStatus,
                        FiscalYear = model.FiscalYearValue,
                        Month = fee.Month
                    };
                    _db.StudentFees.Add(studentFee);

                }
                await _db.SaveChangesAsync();
                TempData["Message"] = "Student payment recorded successfully!";
                TempData["MessageType"] = "success";
                return RedirectToAction("GetStudentsFeeReports");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetStudentsFeeReports()
        {
            try
            {
                var reports = await _db.Students
                    .Include(c => c.SchoolClass)
                    .Include(f => f.StudentFees)
                    .ThenInclude(c => c.FeeType)
                    .ToListAsync();
                if(reports == null)
                    return NoContent();

                var studentFeeReports = reports.Select(student => new StudentFeeDetailsViewModel
                {
                    FullName = student.FullName,
                    ClassName = student.SchoolClass?.ClassName,
                    Address = student.Address,
                    StudentFees = student.StudentFees?.Select(fee => new StudentFee
                    {
                        FeeType = fee.FeeType,
                        Amount = fee.Amount,
                        Month = fee.Month,
                        PaymentDate = fee.PaymentDate
                    }).ToList() ?? new List<StudentFee>()
                }).ToList();
                return View(studentFeeReports);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(Index);
            }
        }

        [HttpGet]   
        public IActionResult GenerateStudentBill()
        {
            List<SchoolClass> schoolClass = _db.SchoolClasses.ToList();
            var model = new StudentFeeViewModel
            {
                /*Students = _db.Students.Select(c => new SelectListItem
                {
                    Value = c.StudentId.ToString(),
                    Text = c.FullName
                }).ToList(),*/
                FiscalYear = _db.FiscalYears.Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name
                }).ToList(),

                FeeTypes = _db.FeeTypes.Select(c => new SelectListItem
                {
                    Value = c.FeeTypeId.ToString(),
                    Text = c.Name
                }).ToList(),

                Class = schoolClass.Select(i => new SelectListItem
                {
                    Value = i.ClassId.ToString(),
                    Text = i.ClassName
                }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        [HttpPost]
public async Task<IActionResult> GenerateStudentBillAsync(StudentFeeViewModel studentFeeView)
{
    if(studentFeeView == null)
    {
        return NotFound();
    }

    try
    {
        var studentInfo = await _db.Students
            .Include(c => c.SchoolClass)
            .Include(f => f.StudentFees)
            .ThenInclude(c => c.FeeType)
            .FirstOrDefaultAsync(c => c.StudentId == studentFeeView.StudentId);

        var selectedFeeTypeIds = studentFeeView.FeeTypeSelections.Select(f => f.FeeTypeId).ToList();

        foreach (var selectedFeeTypeId in selectedFeeTypeIds)
        {
            if (_db.FeeTypes.Any(f => f.FeeTypeId == selectedFeeTypeId) == false)
            {
                ModelState.AddModelError(string.Empty, "Invalid fee type selected.");
                return View(studentFeeView);
            }

            var selectedFeeType = await _db.FeeTypes
                .FirstOrDefaultAsync(f => f.FeeTypeId == selectedFeeTypeId);

            if (selectedFeeType.Name != null)
            {
                // Create a temporary list to hold the new FeeTypeSelections
                var newFeeSelections = new List<FeeTypeSelection>();

                foreach (var months in studentFeeView.FeeTypeSelections)
                {
                    if (months.Month == null || !months.Month.Any())
                    {
                        ModelState.AddModelError(string.Empty, "Please select at least one month.");
                        return View(studentFeeView);
                    }

                    var monthsList = months.Month; // List of selected months
                    foreach (var month in monthsList)
                    {
                        var fee = new FeeTypeSelection
                        {
                            FeeTypeId = selectedFeeTypeId,
                            Amount = months.Amount,
                            Month = month.ToString()
                        };
                        newFeeSelections.Add(fee);
                    }
                }

                // Now add all the new fee selections after the loop ends
                studentFeeView.FeeTypeSelections.AddRange(newFeeSelections);
            }
        }

        var selectedFees = studentInfo.StudentFees
            .Where(f => selectedFeeTypeIds.Contains(f.FeeTypeId) && studentFeeView.FeeTypeSelections
                .Select(fee => fee.Month)
                .Contains(f.Month))
            .ToList();

        var studentBill = new StudentBill
        {
            StudentName = studentInfo.FullName,
            ClassName = studentInfo.SchoolClass?.ClassName,
            Address = studentInfo.Address,
            InvoiceDate = studentFeeView.PaymentDate,
            FeeItems = selectedFees.Select(fee => new FeeItem
            {
                Name = fee.FeeType.Name,
                Amount = fee.Amount
            }).ToList(),
            Amount = selectedFees.Sum(f => f.Amount),
            DiscountAmount = studentFeeView.DiscountAmount,
            TotalAmount = selectedFees.Sum(f => f.Amount) - studentFeeView.DiscountAmount
        };

        return View("PrintStudentBill", studentBill);
    }
    catch (Exception ex)
    {
        // Handle exceptions
        ModelState.AddModelError(string.Empty, "An error occurred while generating the bill.");
        return View(studentFeeView);
    }
}

       /* private async Task<int> GenerateInvoiceNumberAsync()
        {
            int lastInvoice = await _db.StudentBills
                .OrderByDescending(b => b.InvoiceNumber)
                .Select(b => b.InvoiceNumber)
                .FirstOrDefaultAsync();

            return lastInvoice + 1;
        }*/

    }
}
