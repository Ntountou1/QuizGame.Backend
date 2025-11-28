using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuizGame.Domain.Entities;
using QuizGame.Infrastructure.Repositories;
using QuizGame.Application.DTOs;
using System.Security.Claims;
using QuizGame.Application.Interfaces;
using QuizGame.Domain.Interfaces;

namespace QuizGame.Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ILogger _logger;
        private readonly IPlayerRepository _repository;
        private readonly TokenService _tokenService;

        public PlayerService(IPlayerRepository repository, ILogger<PlayerService> logger, TokenService tokenService)
        {
            _repository = repository;
            _logger = logger;
            _tokenService = tokenService;
        }

        //Logs in player
        public LoginResponse? Login(LoginRequest request)
        {
            var player = _repository.GetAllPlayers()
            .FirstOrDefault(p => p.Username == request.Username && p.Password == request.Password);

            if (player == null)
            {
                _logger.LogWarning("Login failed for username {Username}", request.Username);
                return null;
            }

            _logger.LogInformation("Login successful for username {Username}", request.Username);

            // update last login prop of player when he logs in succesfully
            UpdateLastLogin(player.Id);

            var token = _tokenService.GenerateToken(player);
            return new LoginResponse
            {
                Token = token,
                Username = player.Username!,
                UserId = player.Id
            };
        }

        //Return list of all players
        public IEnumerable<PlayerResponse> GetPlayers()
        {
            _logger.LogInformation("Fetching all players from repository");
            var players = _repository.GetAllPlayers();

            var response = players.Select(p => new PlayerResponse
            {
                Id = p.Id,
                Username = p.Username!,
                CreatedAt = p.CreatedAt,
                LastLogInAt = p.LastLogInAt,
                TotalScore = p.TotalScore,
                GamesPlayed = p.GamesPlayed,
                GamesWon = p.GamesWon,
                CurrentRank = p.CurrentRank,
                Level = p.Level
            });

            _logger.LogInformation("Retrieved {Count} players", response.Count());
            return response;
        }

        //Create new player
        public PlayerResponse CreatePlayer(CreatePlayerRequest request)
        {
            _logger.LogInformation("Creating new player {Username} ", request.Username);

            var player = new Player
            {
                Username = request.Username,
                Password = request.Password,
                CreatedAt = DateTime.UtcNow,
                LastLogInAt = null,
                TotalScore = 0,
                GamesPlayed = 0,
                GamesWon = 0,
                CurrentRank = 0,
                Level = 1
            };

            _repository.AddPlayer(player);
            return new PlayerResponse
            {
                Id = player.Id,
                Username = player.Username,
                CreatedAt = player.CreatedAt,
                LastLogInAt= player.LastLogInAt,
                TotalScore = player.TotalScore,
                GamesPlayed = player.GamesPlayed,
                GamesWon= player.GamesWon,
                CurrentRank = player.CurrentRank,
                Level = player.Level
            };
        }

        //Updates password of logged in user given the old password and the new password on the body
        public UpdatePasswordResponse? UpdatePassword(int userId, UpdatePasswordRequest request)
        {
            var player = _repository.GetAllPlayers().FirstOrDefault(p => p.Id == userId);

            if (player == null)
            {
                _logger.LogWarning("Player with id {UserId} not found", userId);
                return null;
            }

            if (player.Password != request.OldPassword)
            {
                _logger.LogWarning("Incorrect old password for user {Username}", player.Username);
                return null;
            }

            player.Password = request.NewPassword;
            _repository.UpdatePlayer(player);
            _logger.LogInformation("Password updated for user {Username}", player.Username);

            return new UpdatePasswordResponse();
        }


        //Automatically update LastLogInAt prop on json when user successfully logs in
        public void UpdateLastLogin (int playerId)
        {
            var player = _repository.GetAllPlayers().FirstOrDefault(p => p.Id == playerId);
            if (player == null)
            {
                var message = $"Player with ID {playerId} not found.";
                _logger.LogError(message);
                throw new KeyNotFoundException(message);
            }

            player.LastLogInAt = DateTime.UtcNow;
            //Update JSON
            _repository.UpdatePlayer(player);
            _logger.LogInformation("Updated LastLogInAt for player {Username}", player.Username);
        }

        public void DeletePlayer (int playerId)
        {
            _logger.LogInformation("Attempting to delete player {PlayerId}", playerId);
            var player = _repository.GetPlayerById(playerId);
            if (player == null)
            {
                _logger.LogWarning("Player {PlayerId} not found", playerId);
                throw new Exception("Player not found");
            }

            _repository.DeletePlayer(playerId);

            _logger.LogInformation("Player {PlayerId} deleted successfully", playerId);
        }
    }
}
