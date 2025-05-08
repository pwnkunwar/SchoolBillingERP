using Microsoft.Extensions.Primitives;

namespace SchoolBillingERP.Models
{
    public class StudentBill
    {
        public int   InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string Address { get; set; }
        public List<FeeItem> FeeItems { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public string FiscalYear { get; set; }
        public string ModeOfPayment { get; set; }
        public string TotalAmountWords { get; set; }
        public string BilledBy { get; set; }
    }
    public class FeeItem
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}
