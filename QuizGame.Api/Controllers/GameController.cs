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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int playerId = int.Parse(userIdClaim.Value);

            var response = _gameService.StartNewGame(playerId);

            return Ok(response);
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
        }
    }
}
