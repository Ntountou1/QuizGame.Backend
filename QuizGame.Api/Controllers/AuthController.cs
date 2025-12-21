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

        /// <summary>
        /// Authenticates a player using their username and password.
        /// </summary>
        /// <param name="request">
        /// The <see cref="LoginRequest"/> containing the player's credentials.
        /// </param>
        /// <returns>
        /// - <c>200 OK</c> with a <see cref="LoginResponse"/> if authentication succeeds.
        /// - <c>400 Bad Request</c> if the request is null or missing required fields.
        /// - <c>401 Unauthorized</c> if the credentials are invalid.
        /// - <c>500 Internal Server Error</c> for unexpected errors.
        /// </returns>
        /// <remarks>
        /// - Calls <see cref="_playerService.Login"/> to perform authentication.
        /// - Returns appropriate HTTP responses based on validation and authentication outcome.
        /// </remarks>
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
