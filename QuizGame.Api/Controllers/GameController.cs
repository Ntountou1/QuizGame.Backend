using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGame.Application.DTOs;
using QuizGame.Application.Interfaces;
using System.Security.Claims;

namespace QuizGame.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Starts a new quiz game session for the currently authenticated player.
        /// </summary>
        /// <returns>
        /// - <c>200 OK</c> with session details if a new game is successfully started.
        /// - <c>400 Bad Request</c> if the game cannot be started (e.g., insufficient questions).
        /// - <c>401 Unauthorized</c> if the user is not authenticated.
        /// - <c>500 Internal Server Error</c> for unexpected errors.
        /// </returns>
        /// <remarks>
        /// - Extracts the player's ID from the authentication claims.
        /// - Calls <see cref="_gameService.StartNewGame"/> to create a new session.
        /// - Requires the user to be authenticated via [Authorize].
        /// </remarks>
        [Authorize]
        [HttpPost("start")]
        public IActionResult StartGame()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User is not authenticated" });
                }

                int playerId = int.Parse(userIdClaim.Value);

                var response = _gameService.StartNewGame(playerId);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // For example, if there are not enough questions to start
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Retrieves the details of a specific game session by its ID.
        /// </summary>
        /// <param name="gameSessionId">The unique identifier of the game session.</param>
        /// <returns>
        /// - <c>200 OK</c> with the session data if found.
        /// - <c>404 Not Found</c> if the session does not exist.
        /// - <c>500 Internal Server Error</c> for unexpected errors.
        /// </returns>
        /// <remarks>
        /// - Calls <see cref="_gameService.GetGameSession"/> to fetch session information.
        /// - Requires the user to be authenticated via [Authorize].
        /// </remarks>
        [Authorize]
        [HttpGet("{gameSessionId}")]
        public IActionResult GetGameSession(int gameSessionId)
        {
            try
            {
                var session = _gameService.GetGameSession(gameSessionId);
                return Ok(session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Submits an answer for a question in an active game session.
        /// </summary>
        /// <param name="request">The <see cref="SubmitAnswerRequest"/> containing the session ID, question ID, and answer ID.</param>
        /// <returns>
        /// - <c>200 OK</c> with the result of the submitted answer and updated session state.
        /// - <c>400 Bad Request</c> if the submission is invalid (e.g., question already answered, session not active).
        /// - <c>404 Not Found</c> if the session or question does not exist.
        /// - <c>500 Internal Server Error</c> for unexpected errors.
        /// </returns>
        /// <remarks>
        /// - Calls <see cref="_gameService.SubmitAnswer"/> to evaluate and process the answer.
        /// - Requires the user to be authenticated via [Authorize].
        /// </remarks>
        [Authorize]
        [HttpPost("submitAnswer")]
        public IActionResult SubmitAnswer([FromBody] SubmitAnswerRequest request)
        {
            try
            {
                var response = _gameService.SubmitAnswer(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

    }
}
