using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models.Entites
{
    public class UserInfo
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();
        public required string Token { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? OfficeLocation { get; set; }
        public string? MobilePhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}
