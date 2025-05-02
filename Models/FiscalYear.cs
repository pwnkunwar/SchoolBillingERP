using System.ComponentModel.DataAnnotations;

namespace SchoolBillingERP.Models
{
    public class FiscalYear
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
