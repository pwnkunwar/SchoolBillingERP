using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolBillingERP.Database;
using SchoolBillingERP.Models;
using System.Diagnostics;

namespace SchoolBillingERP.Controllers
{
    [Authorize]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db,
            ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;

        }
       
        public IActionResult Index()
        {
            int users = _db.Students.Count();
            decimal totalRevenue = _db.StudentFees.Sum(fe => fe.Amount);
            Dashboard dashboard = new Dashboard()
            { 
                totalUsers = users,
                totalRevenue = totalRevenue,
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
