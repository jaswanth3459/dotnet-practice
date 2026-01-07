namespace EmployeeAdminPortal.Models.Entites
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; } // References Employee as customer
        public string CustomerName { get; set; } = string.Empty;

        // Order status enum
        public OrderStatus Status { get; set; }

        // Nested object for shipping address
        public Address ShippingAddress { get; set; } = new Address();

        // Nested object for billing address
        public Address BillingAddress { get; set; } = new Address();

        // Array of order items
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        // Payment information
        public PaymentInfo Payment { get; set; } = new PaymentInfo();

        // Order totals
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }

        // Additional metadata
        public string? Notes { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? TrackingNumber { get; set; }
    }

    // Nested class for Address
    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? ApartmentNumber { get; set; }
    }

    // Nested class for Order Items (array element)
    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public List<string> Attributes { get; set; } = new List<string>(); // e.g., ["Size: Large", "Color: Blue"]
    }

    // Nested class for Payment Info
    public class PaymentInfo
    {
        public PaymentMethod Method { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? Last4Digits { get; set; } // Last 4 digits of card
    }

    // Enums for better type safety
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        PayPal,
        BankTransfer,
        CashOnDelivery
    }

    public enum PaymentStatus
    {
        Pending,
        Authorized,
        Captured,
        Failed,
        Refunded
    }
}
