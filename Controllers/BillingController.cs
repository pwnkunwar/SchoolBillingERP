using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Controllers
{
   [Authorize]
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
            /*if (!ModelState.IsValid)
                return View(model);*/

            try
            {
                

                foreach (var fee in model.FeeTypeSelections)
                {
                    bool feeExists = await _db.StudentFees.AnyAsync(f =>
               f.StudentId == model.StudentId &&
               f.FeeTypeId == fee.FeeTypeId &&
               f.Month == fee.Month &&
               f.FiscalYear == model.FiscalYearValue
           );
                    if(feeExists)
                    {
                        var feeName = await _db.FeeTypes
                            .Where(f => f.FeeTypeId == fee.FeeTypeId)
                            .Select(f => f.Name)
                            .FirstOrDefaultAsync();
                        ModelState.AddModelError(string.Empty, $"A fee with the selected type and month already exists in the system.");
                        return View(model);
                    }
                    var studentFee = new StudentFee
                    {
                        StudentId = model.StudentId,
                        FeeTypeId = fee.FeeTypeId,
                        Amount = fee.Amount,
                        PaymentDate = model.PaymentDate,
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
                    .ThenInclude(f => f.FeeType)
                    .ToListAsync();

                if (reports == null || !reports.Any())
                    return NoContent();

                var studentFeeReports = reports.Select(student => new StudentFeeDetailsViewModel
                {
                    FullName = student.FullName,
                    ClassName = student.SchoolClass?.ClassName,
                    Address = student.Address,
                    StudentFees = student.StudentFees.Select(fee => new StudentFee
                    {
                        FeeType = fee.FeeType,
                        Amount = fee.Amount,
                        Month = fee.Month,
                        PaymentDate = fee.PaymentDate
                    }).ToList()
                }).ToList();

                // Get distinct months & fee types for use in ViewBag
                var allFees = reports.SelectMany(s => s.StudentFees).ToList();
                var months = allFees.Where(f => !string.IsNullOrEmpty(f.Month))
                                    .Select(f => f.Month)
                                    .Distinct()
                                    .OrderBy(m => m)
                                    .ToList();

                var feeTypes = allFees.Select(f => f.FeeType)
                                      .Where(ft => ft != null)
                                      .Distinct()
                                      .ToList();

                var monthlyFeeTypes = feeTypes
                    .Where(ft => allFees.Any(f => f.FeeTypeId == ft.FeeTypeId && !string.IsNullOrEmpty(f.Month)))
                    .ToList();

                var nonMonthlyFeeTypes = feeTypes.Except(monthlyFeeTypes).ToList();

                ViewBag.Months = months;
                ViewBag.MonthlyFeeTypes = monthlyFeeTypes;
                ViewBag.NonMonthlyFeeTypes = nonMonthlyFeeTypes;

                return View(studentFeeReports);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Index");
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
        public async Task<IActionResult> GenerateStudentBillAsync(StudentFeeViewModel studentFeeView)
        {
            if (studentFeeView == null)
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

                if (studentInfo == null)
                {
                    ModelState.AddModelError(string.Empty, "Student not found.");
                    return View(studentFeeView);
                }

                if (studentFeeView.FeeTypeSelections == null)
                {
                    studentFeeView.FeeTypeSelections = new List<FeeTypeSelection>();
                }

                var selectedFeeTypeIds = studentFeeView.FeeTypeSelections
                    .Select(f => f.FeeTypeId)
                    .Distinct()
                    .ToList();

                // Prepare a new list to hold processed fee selections
                var newFeeSelections = new List<FeeTypeSelection>();

                foreach (var selectedFeeTypeId in selectedFeeTypeIds)
                {
                    if (!_db.FeeTypes.Any(f => f.FeeTypeId == selectedFeeTypeId))
                    {
                        ModelState.AddModelError(string.Empty, "Invalid fee type selected.");
                        return View(studentFeeView);
                    }

                    var selectedFeeType = await _db.FeeTypes
                        .FirstOrDefaultAsync(f => f.FeeTypeId == selectedFeeTypeId);

                    if (selectedFeeType?.Name != null)
                    {
                        foreach (var months in studentFeeView.FeeTypeSelections.Where(x => x.FeeTypeId == selectedFeeTypeId))
                        {
                            if (string.IsNullOrWhiteSpace(months.Month) || !months.Month.Any())
                            {
                                newFeeSelections.Add(new FeeTypeSelection
                                {
                                    FeeTypeId = selectedFeeTypeId,
                                    Amount = months.Amount
                                });
                            }
                            else
                            {
                                // Split the comma-separated Month string into individual months
                                var monthList = months.Month.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(m => m.Trim())
                                                      .ToList();

                                foreach (var month in monthList)
                                {
                                    newFeeSelections.Add(new FeeTypeSelection
                                    {
                                        FeeTypeId = selectedFeeTypeId,
                                        Amount = months.Amount,
                                        Month = month
                                    });
                                }
                            }
                        }
                    }
                }

                // Replace original selections with the new processed list
                studentFeeView.FeeTypeSelections = newFeeSelections;

                // Filter student fees based on FeeTypeId and Month
                var selectedFees = studentInfo.StudentFees
                    .Where(f => selectedFeeTypeIds.Contains(f.FeeTypeId) &&
                                studentFeeView.FeeTypeSelections.Select(s => s.Month).Contains(f.Month))
                    .ToList();

                var totalAmount = selectedFees.Sum(f => f.Amount);
                var discount = studentFeeView.DiscountAmount;
                var netTotal = totalAmount - discount;

                var studentBill = new StudentBill
                {
                    StudentName = studentInfo.FullName,
                    ClassName = studentInfo.SchoolClass?.ClassName,
                    Address = studentInfo.Address,
                    InvoiceDate = DateTime.UtcNow.ToString("yyyy/MM/dd"),
                    FeeItems = selectedFees.Select(fee => new FeeItem
                    {
                        Name = fee.FeeType.Name,
                        Amount = fee.Amount
                    }).ToList(),
                    FiscalYear = studentFeeView.FiscalYearValue,
                    ModeOfPayment = studentFeeView.ModeOfPayment,
                    Amount = totalAmount,
                    DiscountAmount = discount,
                    BilledBy = studentFeeView.BilledBy,
                    TotalAmount = netTotal,
                    TotalAmountWords = NumberToWords(netTotal)
                };

                return View("PrintStudentBill", studentBill);
            }
            catch (Exception ex)
            {
                // Log the exception (if you have logging setup)
                ModelState.AddModelError(string.Empty, "An error occurred while generating the bill.");
                return View(studentFeeView);
            }
        }
        [HttpGet]
        public IActionResult EditStudentsFeeReports()
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

        [HttpGet]
        public JsonResult GetStudentInfoJson(Guid classId, Guid studentId, string fiscalYear)
        {
            var student = _db.Students
                .Include(s => s.StudentFees)
                .ThenInclude(f => f.FeeType)
                .FirstOrDefault(s => s.StudentId == studentId && s.ClassId == classId);

            if (student == null)
                return Json(null);

            var studentInfo = new
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                Address = student.Address,
                Fees = student.StudentFees
                    .Where(f => f.FiscalYear == fiscalYear) // adjust this line based on your schema
                    .Select(f => new 
                    {
                        Id = f.FeeTypeId,
                        FeeTypeName = f.FeeType.Name,
                        Month = f.Month,
                        Amount = f.Amount,
                        PaymentDate = f.PaymentDate
                    }).ToList()
            };

            return Json(studentInfo);
        }

        [HttpPost]
        public IActionResult UpdateStudentFees(List<StudentFeeViewModel> fees)
        {
            if (fees == null || !fees.Any())
            {
                return BadRequest("No fees provided.");
            }

            foreach (var fee in fees)
            {
                // First, check if it's a monthly fee based on incoming month value
                var eFee = _db.StudentFees.FirstOrDefault(f => f.FeeTypeId == fee.Id && f.StudentId == fee.StudentId);
                bool isMonthlyFee = !string.IsNullOrEmpty(eFee.Month);

                StudentFee existingFee;

                if (isMonthlyFee)
                {
                    // Match by student, fee type, and month
                    existingFee = _db.StudentFees.FirstOrDefault(f =>
                        f.StudentId == fee.StudentId &&
                        f.FeeTypeId == fee.Id &&
                        f.Month == fee.Month);
                }
                else
                {
                    // Match by student and fee type only
                    existingFee = _db.StudentFees.FirstOrDefault(f =>
                        f.StudentId == fee.StudentId &&
                        f.FeeTypeId == fee.Id &&
                        string.IsNullOrEmpty(f.Month));
                }

                if (existingFee != null)
                {
                    existingFee.Amount = fee.Amount;
                   // existingFee.PaymentDate = fee.PaymentDate; // If you want to update date too
                }
            }

            _db.SaveChanges();
            return Ok("Fees updated successfully.");
        }
        [HttpPost]
        public IActionResult DeleteStudentFee(Guid id, Guid studentId, string month)
        {
            if(id == Guid.Empty || studentId == Guid.Empty)
            {
                return BadRequest("Invalid input.");
            }
            if (string.IsNullOrEmpty(month))
            {
                var feeDetails = _db.StudentFees.FirstOrDefault(fe => fe.StudentId == studentId && fe.FeeTypeId == id);
                if (feeDetails == null)
                {
                    return NotFound();
                }
                _db.StudentFees.Remove(feeDetails);

            }
            else
            {
                var monthlyFeeDetails = _db.StudentFees.FirstOrDefault(fe => fe.StudentId == studentId && fe.FeeTypeId == id && fe.Month == month);
                if (monthlyFeeDetails == null)
                {
                    return NotFound();
                }
                _db.StudentFees.Remove(monthlyFeeDetails);
            }
            _db.SaveChanges();

            /*var fee = _db.StudentFees.FirstOrDefault(f => f. == id);
            if (fee == null) return NotFound();

            _db.StudentFees.Remove(fee);
            _db.SaveChanges();
*/
            return Ok();
        }



        /* private async Task<int> GenerateInvoiceNumberAsync()
         {
             int lastInvoice = await _db.StudentBills
                 .OrderByDescending(b => b.InvoiceNumber)
                 .Select(b => b.InvoiceNumber)
                 .FirstOrDefaultAsync();

             return lastInvoice + 1;
         }*/
        public static string NumberToWords(decimal number)
        {
            if (number == 0)
                return "Zero Only";

            var parts = number.ToString("0.00").Split('.');
            int rupees = int.Parse(parts[0]);
            int paisa = int.Parse(parts[1]);

            string words = "";

            if (rupees > 0)
                words += $"{NumberToWordsInt(rupees)} Rupees";

            if (paisa > 0)
                words += $" and {NumberToWordsInt(paisa)} Paisa";

            return words + " Only";
        }

        private static string NumberToWordsInt(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + NumberToWordsInt(Math.Abs(number));

            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                          "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };

            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToWordsInt(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToWordsInt(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWordsInt(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWordsInt(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (!string.IsNullOrEmpty(words))
                    words += "and ";

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }



    }
}
