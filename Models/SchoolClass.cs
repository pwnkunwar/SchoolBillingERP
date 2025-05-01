using System.ComponentModel.DataAnnotations;

namespace SchoolBillingERP.Models
{
    public class SchoolClass
    {
        [Key]
        public Guid ClassId { get; set; }
        [Required]
        public string ClassName { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();

    }
}
