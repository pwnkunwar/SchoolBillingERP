using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Controllers
{
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
    }
}
