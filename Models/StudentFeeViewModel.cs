using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolBillingERP.Models
{
    public class StudentFeeViewModel
    {
        public Guid StudentId { get; set; }
        public Guid Id { get; set; }
        public List<SelectListItem> Class { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FeeTypes { get; set; } = new List<SelectListItem>();
        public List<FeeTypeSelection> FeeTypeSelections { get; set; } = new List<FeeTypeSelection>();
        public List<SelectListItem> FiscalYear { get; set; } = new List<SelectListItem>();
        public decimal Amount { get; set; }
        public string PaymentDate { get; set; }
        public string FeeStatus { get; set; }
        public string FiscalYearValue { get; set; }
        public decimal DiscountAmount { get; set; }

    }
    public class FeeTypeSelection
    {
        public Guid FeeTypeId { get; set; }
        public decimal Amount { get; set; }
        public string? Month { get; set; }

    }
}
