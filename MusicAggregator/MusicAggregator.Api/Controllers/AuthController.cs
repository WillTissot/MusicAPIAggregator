using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicAggregator.Application.Auth;

namespace MusicAggregator.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _token;

        public AuthController(ITokenService token)
        {
            _token = token;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public IActionResult GetToken([FromBody] LoginRequest req)
        {
            if (req is { Username: "test", Password: "test" })
                return Ok(new { token = _token.CreateToken(req.Username) });

            return Unauthorized();
        }
    }
}
