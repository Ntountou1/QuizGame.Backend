using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuizGame.Domain.Entities;
using QuizGame.Infrastructure.Repositories;
using QuizGame.Application.DTOs;

namespace QuizGame.Application.Services
{
    public class PlayerService
    {
        private readonly ILogger _logger;
        private readonly PlayerRepository _repository;
        private readonly TokenService _tokenService;

        public PlayerService(PlayerRepository repository, ILogger<PlayerService> logger, TokenService tokenService)
        {
            _repository = repository;
            _logger = logger;
            _tokenService = tokenService;
        }

        public LoginResponse? Login (LoginRequest request)
        {
            var player = _repository.GetAllPlayers()
            .FirstOrDefault(p => p.Username == request.Username && p.Password == request.Password);

            if (player == null) {
                _logger.LogWarning("Login failed for username {Username}", request.Username);
                return null;
            }

            _logger.LogInformation("Login successful for username {Username}", request.Username);

            var token = _tokenService.GenerateToken(player);
            return new LoginResponse
            {
                Token = token,
                Username = player.Username!,
                UserId = player.Id
            };
        }

        public IEnumerable<PlayerResponse> GetPlayers()
        {
            _logger.LogInformation("Fetching all players from repository");
            var players = _repository.GetAllPlayers();

            var response = players.Select(p => new PlayerResponse
            {
                Id = p.Id,
                Username = p.Username!
            });

            _logger.LogInformation("Retrieved {Count} players", response.Count());
            return response;
        }

        public PlayerResponse CreatePlayer(CreatePlayerRequest request)
        {
            _logger.LogInformation("Creating new player {Username} ", request.Username);

            var player = new Player
            {
                Username = request.Username,
                Password = request.Password,
            };

            _repository.AddPlayer(player);
            return new PlayerResponse
            {
                Id = player.Id,
                Username = player.Username
            };
        }
    }
}
