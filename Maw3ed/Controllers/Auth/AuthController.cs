using Maw3ed.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maw3ed.APIs
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;

        public AuthController(IAuthManager authManager)
        {
            _authManager = authManager;
        }

        // POST: api/auth/register
        // Works for both Patient and Doctor — the "Role" field in the body decides which.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authManager.RegisterAsync(dto);

            if (!result.IsAuthenticated)
                return BadRequest(result.Errors);

            return Ok(result);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authManager.LoginAsync(dto);

            if (!result.IsAuthenticated)
                return Unauthorized(result.Errors);

            return Ok(result);
        }

        //GET:api/auth/confirm-email?userId=USER_ID_HERE&token=TOKEN_HERE
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid confirmation link.");

            var result = await _authManager.ConfirmEmailAsync(new ConfirmEmailDto
            {
                UserId = userId,
                Token = token
            });

            if (!result.IsAuthenticated)
                return BadRequest(result.Errors);

            // Returns JWT directly — user is now logged in.
            return Ok(result);
        }

        //logout
        // POST: api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully." });
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authManager.ForgotPasswordAsync(dto);
            // Always 200 — don't reveal if email exists.
            return Ok(result.Errors);
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authManager.ResetPasswordAsync(dto);

            if (result.Errors.Any(e => e.Contains("successfully")))
                return Ok(result.Errors);

            return BadRequest(result.Errors);
        }
    }
}
