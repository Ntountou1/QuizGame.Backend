using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuizGame.Domain.Entities;
using QuizGame.Infrastructure.Repositories;

namespace QuizGame.Application.Services
{
    public class PlayerService
    {
        private readonly ILogger _logger;
        private readonly PlayerRepository _repository;

        public PlayerService(PlayerRepository repository, ILogger<PlayerService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public IEnumerable<Player> GetPlayers()
        {
            _logger.LogInformation("Fetching all players from repository");
            var players = _repository.GetAllPlayers();
            _logger.LogInformation("Retrieved {Count} players", players.Count());
            return players;
        }
    }
}
