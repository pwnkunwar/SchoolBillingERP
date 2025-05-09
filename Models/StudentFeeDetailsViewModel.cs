namespace SchoolBillingERP.Models
{
    public class StudentFeeDetailsViewModel
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; }
        public string ClassName { get; set; }
        public string Address { get; set; }
        public List<StudentFee> StudentFees { get; set; }
    }
}
