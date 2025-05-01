using Microsoft.EntityFrameworkCore;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }
       // public DbSet<Student> Students { get; set; }
        public DbSet<SchoolClass> SchoolClasses { get; set; }
        public DbSet<FeeType> FeeTypes { get; set; }


    }
}
