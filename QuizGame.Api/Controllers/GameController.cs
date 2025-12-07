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
