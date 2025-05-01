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
    }
}
