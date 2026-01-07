using EmployeeAdminPortal.Models.Entites;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Data
{
    public class ApplicationDbContex : DbContext
    {
        public ApplicationDbContex(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToContainer("Employees");
            modelBuilder.Entity<Employee>().HasPartitionKey(e => e.EmployeeId);
            modelBuilder.Entity<Order>().ToContainer("Orders");
            modelBuilder.Entity<Order>().HasPartitionKey(o => o.OrderId);
            modelBuilder.Entity<Order>().OwnsOne(o => o.ShippingAddress);
            modelBuilder.Entity<Order>().OwnsOne(o => o.BillingAddress);
            modelBuilder.Entity<Order>().OwnsOne(o => o.Payment);
            modelBuilder.Entity<Order>().OwnsMany(o => o.Items);
           
        }
    }
}
