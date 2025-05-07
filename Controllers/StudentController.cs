using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;
using System.Security.AccessControl;

namespace SchoolBillingERP.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _db;
        public StudentController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            ViewData["ClassList"] = new SelectList(_db.SchoolClasses, "ClassId", "ClassName");
            return View();
        }
        public async Task<IActionResult> CreateAsync(Student student)
        {
           /* if (!ModelState.IsValid)
                return View(student);*/

            try
            {
                var className = _db.SchoolClasses
                    .FirstOrDefault(c => c.ClassId == student.ClassId);
                if(className == null)
                {
                    return NotFound();
                }
                await _db.Students.AddAsync(student);
                await _db.SaveChangesAsync();

                TempData["Message"] = "Student Added successfully!";
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Consider logging the error instead of writing to console in production
                // Example: _logger.LogError(ex, "Error creating class.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> GetAllStudentsAsync()
        {
            try
            {
                List<Student> students = await _db.Students
    .Include(s => s.SchoolClass) // Include the related SchoolClass data
    .ToListAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                // Consider logging the error instead of writing to console in production
                // Example: _logger.LogError(ex, "Error creating class.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the class.");
                return RedirectToAction(nameof(Index));
            }
        }
       
        public IActionResult EditStudentDetails(Guid id)
        {
            if(id == Guid.Empty)
            {
                return NotFound();
            }   
            try
            {
                var student = _db.Students
                    .Include(s => s.SchoolClass)
                    .FirstOrDefault(s => s.StudentId == id);
                if (student == null)
                {
                    return NotFound();
                }
                ViewData["ClassList"] = new SelectList(_db.SchoolClasses, "ClassId", "ClassName");
                return View(student);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditStudentDetails(Student student)
        {
            if (student == null)
            {
                return NotFound();
            }
            try
            {
                var updateStudent = new Student()
                { 
                    StudentId = student.StudentId,
                    FullName = student.FullName,
                    Address = student.Address,
                    Gender = student.Gender,
                    ParentPhoneNumber = student.ParentPhoneNumber,
                    Status = student.Status,
                    ClassId = student.ClassId
                };
                _db.Students.Update(updateStudent);
               
                await _db.SaveChangesAsync();
                TempData["Message"] = "Student Edited successfully!";
                TempData["MessageType"] = "success";
                return RedirectToAction("GetAllStudents");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return View();
            }
        }
        public async Task<IActionResult> DeleteStudentDetails(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }
            try
            {
                var student = await _db.Students.FindAsync(id);
                if (student == null)
                {
                    return NotFound();
                }
                _db.Students.Remove(student);
                await _db.SaveChangesAsync();
                TempData["Message"] = "Student Deleted successfully!";
                TempData["MessageType"] = "success";
                return RedirectToAction("GetAllStudents");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return View();
            }
        }

    }
}
