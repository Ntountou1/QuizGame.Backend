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
            var response = _playerService.Login(request);

            if (response == null)
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(response);
        }
    }
}
