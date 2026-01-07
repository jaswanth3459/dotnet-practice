using EmployeeAdminPortal.Models.Entites;

namespace EmployeeAdminPortal.Models
{
    public class AddOrderDto
    {
        public required Guid CustomerId { get; set; }
        public required string CustomerName { get; set; }
        public required Address ShippingAddress { get; set; }
        public required Address BillingAddress { get; set; }
        public required List<OrderItem> Items { get; set; }
        public required PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public List<string>? Tags { get; set; }
    }
}
