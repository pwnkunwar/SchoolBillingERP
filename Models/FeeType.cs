using System.ComponentModel.DataAnnotations;

namespace SchoolBillingERP.Models
{
    public class FeeType
    {
        [Key]
        public Guid FeeTypeId { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
