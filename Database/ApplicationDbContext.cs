using Microsoft.EntityFrameworkCore;
using SchoolBillingERP.Models;

namespace SchoolBillingERP.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<SchoolClass> SchoolClasses { get; set; }
        public DbSet<FeeType> FeeTypes { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<FiscalYear> FiscalYears { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the relationship between Student and SchoolClass
            modelBuilder.Entity<Student>()
                .HasOne(s => s.SchoolClass)
                .WithMany()
                .HasForeignKey(s => s.ClassId);

           
            modelBuilder.Entity<StudentFee>()
                .HasOne(sf => sf.FeeType)
                .WithMany()
                .HasForeignKey(sf => sf.FeeTypeId);
        }

    }
}
