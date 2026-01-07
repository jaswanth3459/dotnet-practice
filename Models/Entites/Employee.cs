namespace EmployeeAdminPortal.Models.Entites
{
    public class Employee
    {
        public Guid EmployeeId { get; set; } = Guid.NewGuid();
        public required string Name { get; set; } = null!;
        public required string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public required decimal Salary { get; set; }
    }
}
