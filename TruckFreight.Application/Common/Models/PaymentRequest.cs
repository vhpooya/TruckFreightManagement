namespace TruckFreight.Application.Common.Models
{
    public class PaymentRequest
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
} 