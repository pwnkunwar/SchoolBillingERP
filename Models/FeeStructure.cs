using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SchoolBillingERP.Models
{
    public class FeeStructure
    {
        [Key]
        public Guid FeeStructureId { get; set; }
        public Guid ClassId { get; set; }
        [ForeignKey("ClassId")]
        public SchoolClass Class { get; set; }
        [Required]
        public string FeeType { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
