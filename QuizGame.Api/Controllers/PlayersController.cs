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

        /// <summary>
        /// Retrieves a list of all registered players.
        /// </summary>
        /// <returns>
        /// - <c>200 OK</c> with a collection of <see cref="PlayerResponse"/> objects.
        /// - <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <remarks>
        /// - Requires the user to be authenticated via [Authorize].
        /// - Calls <see cref="_playerService.GetPlayers"/> to fetch player data.
        /// </remarks>
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

        // <summary>
        /// Creates a new player account.
        /// </summary>
        /// <param name="request">The <see cref="CreatePlayerRequest"/> containing the username and password.</param>
        /// <returns>
        /// - <c>201 Created</c> with the created <see cref="PlayerResponse"/> if successful.
        /// - <c>400 Bad Request</c> if the request is null or missing required fields.
        /// - <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <remarks>
        /// - Calls <see cref="_playerService.CreatePlayer"/> to add the new player.
        /// - Returns a location header referencing <see cref="GetAll"/> for convenience.
        /// - Requires the user to be authenticated via [Authorize].
        /// </remarks>
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

        /// <summary>
        /// Updates the password of the currently authenticated player.
        /// </summary>
        /// <param name="request">The <see cref="UpdatePasswordRequest"/> containing the old and new passwords.</param>
        /// <returns>
        /// - <c>200 OK</c> if the password is successfully updated.
        /// - <c>400 Bad Request</c> if the old password is incorrect or user not found.
        /// - <c>401 Unauthorized</c> if the user is not authenticated.
        /// - <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <remarks>
        /// - Extracts the player's ID from the authentication claims.
        /// - Calls <see cref="_playerService.UpdatePassword"/> to perform the update.
        /// - Requires the user to be authenticated via [Authorize].
        /// </remarks>
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

        /// <summary>
        /// Deletes the currently authenticated player from the system.
        /// </summary>
        /// <returns>
        /// - <c>200 OK</c> if the player is successfully deleted.
        /// - <c>401 Unauthorized</c> if the user is not authenticated.
        /// - <c>404 Not Found</c> if the player does not exist.
        /// - <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <remarks>
        /// - Extracts the player's ID from the authentication claims.
        /// - Calls <see cref="_playerService.DeletePlayer"/> to remove the player.
        /// - Requires the user to be authenticated via [Authorize].
        /// </remarks>
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
