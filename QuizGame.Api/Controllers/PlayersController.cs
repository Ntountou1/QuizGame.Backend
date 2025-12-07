using Microsoft.AspNetCore.Mvc;
using QuizGame.Application.Services;
using Microsoft.AspNetCore.Authorization;
using QuizGame.Domain.Entities;
using QuizGame.Application.DTOs;
using QuizGame.Application.Interfaces;

namespace QuizGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                _logger.LogInformation("GET /api/players called");
                var players = _playerService.GetPlayers();
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching players");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize]
        [HttpPost("createNewPlayer")]
        public IActionResult CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                var createdPlayer = _playerService.CreatePlayer(request);
                return CreatedAtAction(nameof(GetAll), new { id = createdPlayer.Id }, createdPlayer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating player");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize]
        [HttpPut("updatePassword")]
        public IActionResult UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new { message = "User not authenticated" });

                int userId = int.Parse(userIdClaim.Value);
                var response = _playerService.UpdatePassword(userId, request);

                if (response == null)
                {
                    return BadRequest(new { message = "Old password is incorrect or user not found" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [Authorize]
        [HttpDelete("deletePlayer")]
        public IActionResult DeletePlayer()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new { message = "User not authenticated" });

                int playerId = int.Parse(userIdClaim.Value);
                _playerService.DeletePlayer(playerId);

                return Ok(new { message = "User deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Player not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting player");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

    }
}
