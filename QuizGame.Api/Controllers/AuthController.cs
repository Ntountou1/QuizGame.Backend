using Microsoft.AspNetCore.Mvc;
using QuizGame.Application.Services;

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
            var token = _playerService.Login(request);

            if (token == null)
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(new {Token = token});
        }
    }
}
