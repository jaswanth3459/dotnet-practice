using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContex dbContext;

        public OrdersController(ApplicationDbContex dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await dbContext.Orders
                .AsNoTracking()
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Orders/{id}
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            return Ok(order);
        }

        // GET: api/Orders/customer/{customerId}
        [HttpGet]
        [Route("customer/{customerId:guid}")]
        public async Task<IActionResult> GetOrdersByCustomer(Guid customerId)
        {
            var orders = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Orders/status/{status}
        [HttpGet]
        [Route("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder(AddOrderDto addOrderDto)
        {
            // Calculate order totals
            decimal subtotal = 0;
            foreach (var item in addOrderDto.Items)
            {
                item.TotalPrice = (item.UnitPrice * item.Quantity) - item.Discount;
                subtotal += item.TotalPrice;
            }

            decimal tax = subtotal * 0.10m; // 10% tax
            decimal shippingCost = 15.00m; // Flat shipping cost
            decimal totalAmount = subtotal + tax + shippingCost;

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                CustomerId = addOrderDto.CustomerId,
                CustomerName = addOrderDto.CustomerName,
                Status = OrderStatus.Pending,
                ShippingAddress = addOrderDto.ShippingAddress,
                BillingAddress = addOrderDto.BillingAddress,
                Items = addOrderDto.Items,
                Payment = new PaymentInfo
                {
                    Method = addOrderDto.PaymentMethod,
                    PaymentStatus = PaymentStatus.Pending
                },
                SubTotal = subtotal,
                Tax = tax,
                ShippingCost = shippingCost,
                TotalAmount = totalAmount,
                Notes = addOrderDto.Notes,
                Tags = addOrderDto.Tags ?? new List<string>()
            };

            await dbContext.Orders.AddAsync(order);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
        }

        // PUT: api/Orders/{id}
        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateOrder(Guid id, UpdateOrderDto updateOrderDto)
        {
            var order = await dbContext.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            // Update only provided fields
            if (updateOrderDto.Status.HasValue)
            {
                order.Status = updateOrderDto.Status.Value;

                // Auto-update dates based on status
                if (updateOrderDto.Status == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                {
                    order.ShippedDate = DateTime.UtcNow;
                }
                else if (updateOrderDto.Status == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                {
                    order.DeliveredDate = DateTime.UtcNow;
                }
            }

            if (updateOrderDto.ShippingAddress != null)
                order.ShippingAddress = updateOrderDto.ShippingAddress;

            if (updateOrderDto.BillingAddress != null)
                order.BillingAddress = updateOrderDto.BillingAddress;

            if (updateOrderDto.Items != null)
            {
                order.Items = updateOrderDto.Items;

                // Recalculate totals
                decimal subtotal = 0;
                foreach (var item in order.Items)
                {
                    item.TotalPrice = (item.UnitPrice * item.Quantity) - item.Discount;
                    subtotal += item.TotalPrice;
                }

                order.SubTotal = subtotal;
                order.Tax = subtotal * 0.10m;
                order.TotalAmount = order.SubTotal + order.Tax + order.ShippingCost;
            }

            if (updateOrderDto.PaymentStatus.HasValue)
            {
                order.Payment.PaymentStatus = updateOrderDto.PaymentStatus.Value;
                if (updateOrderDto.PaymentStatus == PaymentStatus.Captured)
                {
                    order.Payment.PaymentDate = DateTime.UtcNow;
                }
            }

            if (updateOrderDto.Notes != null)
                order.Notes = updateOrderDto.Notes;

            if (updateOrderDto.Tags != null)
                order.Tags = updateOrderDto.Tags;

            if (updateOrderDto.TrackingNumber != null)
                order.TrackingNumber = updateOrderDto.TrackingNumber;

            if (updateOrderDto.ShippedDate.HasValue)
                order.ShippedDate = updateOrderDto.ShippedDate;

            if (updateOrderDto.DeliveredDate.HasValue)
                order.DeliveredDate = updateOrderDto.DeliveredDate;

            await dbContext.SaveChangesAsync();

            return Ok(order);
        }

        // DELETE: api/Orders/{id}
        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await dbContext.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            dbContext.Orders.Remove(order);
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Order deleted successfully", orderId = id });
        }

        // Helper method to generate order numbers
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        // PATCH: api/Orders/{id}/cancel
        [HttpPatch]
        [Route("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var order = await dbContext.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            if (order.Status == OrderStatus.Delivered)
            {
                return BadRequest("Cannot cancel a delivered order.");
            }

            order.Status = OrderStatus.Cancelled;
            await dbContext.SaveChangesAsync();

            return Ok(order);
        }
    }
}
