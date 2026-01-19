using Microsoft.AspNetCore.Mvc;
using QuizGame.Application.DTOs;
using QuizGame.Application.Services;
using QuizGame.Infrastructure.Repositories;

namespace QuizGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PlayerService _playerService;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthController(PlayerService playerService, RefreshTokenService refreshTokenService)
        {
            _playerService = playerService;
            _refreshTokenService = refreshTokenService;
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

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { message = "Refresh token is required" });

            // Validate the refresh token
            var refreshToken = _refreshTokenService.ValidateRefreshToken(request.RefreshToken);
            if (refreshToken == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            // Get player from repository
            var player = _playerService.GetPlayerById(refreshToken.PlayerId);
            if (player == null)
                return Unauthorized();

            // Generate new JWT
            var newToken = _playerService.GenerateJwtToken(player);

            return Ok(new
            {
                Token = newToken,
                RefreshToken = refreshToken.Token, // for now, we keep the same token
                Username = player.Username,
                UserId = player.Id
            });
        }
    }
}
