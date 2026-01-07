using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace JwtDecodeExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwtController : ControllerBase
    {
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
    }
}
