using EmployeeAdminPortal.Models.Entites;

namespace EmployeeAdminPortal.Models
{
    public class UpdateOrderDto
    {
        public OrderStatus? Status { get; set; }
        public Address? ShippingAddress { get; set; }
        public Address? BillingAddress { get; set; }
        public List<OrderItem>? Items { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public string? Notes { get; set; }
        public List<string>? Tags { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
    }
}
