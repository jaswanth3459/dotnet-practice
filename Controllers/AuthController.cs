using EmployeeAdminPortal.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContex dbContex;

        public AuthController(ApplicationDbContex dbContex)
        {
            this.dbContex = dbContex;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var validationErrors = new List<string>();

            // Email Validation
            if (string.IsNullOrEmpty(request.Email))
            {
                validationErrors.Add("Email is required.");
            }
            else if (string.IsNullOrWhiteSpace(request.Email))
            {
                validationErrors.Add("Email cannot be empty or contain only whitespace.");
            }
            else if (request.Email.Contains(" "))
            {
                validationErrors.Add("Email cannot contain spaces.");
            }
            else if (!IsValidEmail(request.Email))
            {
                validationErrors.Add("Email format is invalid. Please provide a valid email address.");
            }
            else if (request.Email.Length > 255)
            {
                validationErrors.Add("Email cannot exceed 255 characters.");
            }

            // Password Validation
            if (string.IsNullOrEmpty(request.Password))
            {
                validationErrors.Add("Password is required.");
            }
            else if (string.IsNullOrWhiteSpace(request.Password))
            {
                validationErrors.Add("Password cannot be empty or contain only whitespace.");
            }
            else if (request.Password.Length < 6)
            {
                validationErrors.Add("Password must be at least 6 characters long.");
            }
            else if (request.Password.Length > 100)
            {
                validationErrors.Add("Password cannot exceed 100 characters.");
            }
            else if (request.Password.StartsWith(" ") || request.Password.EndsWith(" "))
            {
                validationErrors.Add("Password cannot start or end with spaces.");
            }

            // If there are validation errors, return BadRequest
            if (validationErrors.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    errors = validationErrors
                });
            }

            // Validation passed - proceed with your logic
            // Example: Check if user exists in database
            var user = dbContex.Employees.FirstOrDefault(e => e.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid email or password."
                });
            }

            // Here you would typically verify the password (hash comparison)
            // For now, returning success
            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    email = user.Email,
                    name = user.Name
                }
            });
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Basic email regex pattern
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }
    }

    // Simple request model without validation attributes
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
