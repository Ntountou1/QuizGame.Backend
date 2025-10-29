using Microsoft.AspNetCore.Mvc;
using QuizGame.Application.Services;

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

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GET /api/players called");
            var players = _playerService.GetPlayers();
            return Ok(players);
        }
    }
}
