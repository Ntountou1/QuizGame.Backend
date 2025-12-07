using Microsoft.AspNetCore.Mvc;
using QuizGame.Application.Services;
using QuizGame.Application.DTOs;

namespace QuizGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PlayerService _playerService;

        public AuthController(PlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                var response = _playerService.Login(request);

                if (response == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
