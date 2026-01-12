using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entites;

namespace JwtDecodeExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwtController : ControllerBase
    {
        private readonly ApplicationDbContex _context;

        public JwtController(ApplicationDbContex context)
        {
            _context = context;
        }
        [HttpGet("decode")]
        public IActionResult DecodeJwtToken([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();

                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return BadRequest("Invalid token.");
                }

                var claims = jsonToken?.Claims;

                var claimsDictionary = claims.ToDictionary(c => c.Type, c => c.Value);

                return Ok(claimsDictionary);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error decoding token: {ex.Message}");
            }
        }

        [HttpPost("decode-and-save")]
        public async Task<IActionResult> DecodeAndSaveToken([FromBody] DecodeTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Step 1: Decode the JWT token using same logic as GET endpoint
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(request.Token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return BadRequest("Invalid token.");
                }

                // Step 2: Check if token already exists in database
                var existingToken = _context.UserInfos.FirstOrDefault(u => u.Token == request.Token);
                if (existingToken != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This token has already been used. Please use a different token or update the existing data using PATCH endpoint."
                    });
                }

                // Step 3: Extract claims from the token
                var claims = jsonToken.Claims;
                var claimsDictionary = claims.ToDictionary(c => c.Type, c => c.Value);

                var userInfo = new UserInfo
                {
                    UserId = Guid.NewGuid(),
                    Token = request.Token,
                    Email = GetClaimValue(claimsDictionary, "email", "upn", "unique_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "preferred_username")
                            ?? "unknown@email.com",
                    Name = GetClaimValue(claimsDictionary, "name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                           ?? "Unknown",
                    GivenName = GetClaimValue(claimsDictionary, "given_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"),
                    Surname = GetClaimValue(claimsDictionary, "family_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"),
                    JobTitle = GetClaimValue(claimsDictionary, "jobTitle", "job_title"),
                    Department = GetClaimValue(claimsDictionary, "department"),
                    OfficeLocation = GetClaimValue(claimsDictionary, "officeLocation", "office_location"),
                    MobilePhone = GetClaimValue(claimsDictionary, "mobilePhone", "mobile_phone", "phone_number"),
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                // Step 5: Save to Cosmos DB using Entity Framework
                _context.UserInfos.Add(userInfo);
                await _context.SaveChangesAsync();

                // Step 6: Return the saved user info along with all decoded claims
                return Ok(new
                {
                    message = "Token decoded and saved successfully",
                    userId = userInfo.UserId,
                    userInfo = userInfo,
                    decodedClaims = claimsDictionary
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing token: {ex.Message}");
            }
        }

        [HttpPatch("update-token-data")]
        public async Task<IActionResult> UpdateTokenData([FromBody] DecodeTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Step 1: Check if token exists in database
                var existingUserInfo = _context.UserInfos.FirstOrDefault(u => u.Token == request.Token);

                if (existingUserInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Token not found. Please use an existing token or create a new entry using POST endpoint."
                    });
                }

                // Step 2: Decode the JWT token to get updated claims
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(request.Token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return BadRequest("Invalid token.");
                }

                // Step 3: Extract claims from the token
                var claims = jsonToken.Claims;
                var claimsDictionary = claims.ToDictionary(c => c.Type, c => c.Value);

                // Step 4: Update the existing user info with new data from token
                existingUserInfo.Email = GetClaimValue(claimsDictionary, "email", "upn", "unique_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "preferred_username")
                        ?? existingUserInfo.Email;
                existingUserInfo.Name = GetClaimValue(claimsDictionary, "name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                       ?? existingUserInfo.Name;
                existingUserInfo.GivenName = GetClaimValue(claimsDictionary, "given_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
                existingUserInfo.Surname = GetClaimValue(claimsDictionary, "family_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
                existingUserInfo.JobTitle = GetClaimValue(claimsDictionary, "jobTitle", "job_title");
                existingUserInfo.Department = GetClaimValue(claimsDictionary, "department");
                existingUserInfo.OfficeLocation = GetClaimValue(claimsDictionary, "officeLocation", "office_location");
                existingUserInfo.MobilePhone = GetClaimValue(claimsDictionary, "mobilePhone", "mobile_phone", "phone_number");
                existingUserInfo.LastLoginAt = DateTime.UtcNow;

                // Step 5: Save changes to database
                await _context.SaveChangesAsync();

                // Step 6: Return the updated user info along with all decoded claims
                return Ok(new
                {
                    message = "Token data updated successfully",
                    userId = existingUserInfo.UserId,
                    userInfo = existingUserInfo,
                    decodedClaims = claimsDictionary
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating token data: {ex.Message}");
            }
        }

        private string? GetClaimValue(Dictionary<string, string> claims, params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                if (claims.TryGetValue(claimType, out var value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            return null;
        }
    }
}
