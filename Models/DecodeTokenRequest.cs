using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Models
{
    public class DecodeTokenRequest
    {
        [Required(ErrorMessage = "Token is required.")]
        public required string Token { get; set; }
    }
}
