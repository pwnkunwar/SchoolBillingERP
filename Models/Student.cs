using System.ComponentModel.DataAnnotations;

namespace SchoolBillingERP.Models
{
    public class Student
    {
        [Key]
        public Guid StudentId { get; set; }
        [Required]
        public string FullName { get; set; }



    }
}
