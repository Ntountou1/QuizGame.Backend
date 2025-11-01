using Microsoft.AspNetCore.Mvc;
using QuizGame.Application.Services;
using Microsoft.AspNetCore.Authorization;
using QuizGame.Domain.Entities;

namespace QuizGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController: ControllerBase
    {
        private readonly PlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(PlayerService playerService, ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GET /api/players called");
            var players = _playerService.GetPlayers();
            return Ok(players);
        }

        [Authorize]
        [HttpPost("createNewPlayer")]
        public IActionResult CreatePlayer([FromBody] Player player)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.Username) || string.IsNullOrWhiteSpace(player.Password))
            {
                return BadRequest("Username and password are required");
            } 
            var createdPlayer = _playerService.CreatePlayer(player);
            return CreatedAtAction(nameof(GetAll), new { id = createdPlayer.Id }, createdPlayer);
        } 
    }
}
