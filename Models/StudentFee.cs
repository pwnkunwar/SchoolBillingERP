using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolBillingERP.Models
{
    public class StudentFee
    {
        [Key]
        public Guid StudentFeeId { get; set; }
        public Guid FeeTypeId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentDate { get; set; }
        public string FeeStatus { get; set; }
        public string FiscalYear { get; set; }
        public string? Month { get; set; }

        public FeeType FeeType { get; set; }

        public Guid StudentId { get; set; }
        public Student Student { get; set; }

    }
}
