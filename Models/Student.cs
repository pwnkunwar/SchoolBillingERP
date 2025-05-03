using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolBillingERP.Models
{
    public class Student
    {
        [Key]
        public Guid StudentId { get; set; }
        [Required]
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string ParentPhoneNumber { get; set; }   
        public Guid ClassId { get; set; }
        public string Status { get; set; }

        //Navigation Properties
        public SchoolClass? SchoolClass { get; set; }
        public ICollection<StudentFee> StudentFees { get; set; }
    }
}
