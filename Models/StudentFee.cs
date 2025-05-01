using System.ComponentModel.DataAnnotations;

namespace SchoolBillingERP.Models
{
    public class StudentFee
    {
        [Key]
        public Guid StudentFeeId { get; set; }
        public Guid StudentId { get; set; }
        public Guid FeeTypeId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentDate { get; set; }
        public string FeeStatus { get; set; }

    }
}
