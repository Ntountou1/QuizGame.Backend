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
        private readonly RefreshTokenService _refreshTokenService;

        public PlayerService(IPlayerRepository repository, ILogger<PlayerService> logger, TokenService tokenService, RefreshTokenService refreshTokenService)
        {
            _repository = repository;
            _logger = logger;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        /// <summary>
        /// Authenticates a player using username and password credentials.
        /// </summary>
        /// <param name="request">
        /// The login request containing the player's credentials.
        /// </param>
        /// <returns>
        /// A <see cref="LoginResponse"/> containing authentication data if the credentials
        /// are valid; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// - Validates credentials against the player repository.
        /// - Updates the player's last login timestamp on successful authentication.
        /// - Generates and returns an authentication token.
        /// </remarks>
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

            var refreshToken = _refreshTokenService.GenerateRefreshToken(player.Id);

            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                Username = player.Username!,
                UserId = player.Id
            };
        }

        /// <summary>
        /// Retrieves all registered players.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="PlayerResponse"/> objects
        /// representing all players in the system.
        /// </returns>
        /// <remarks>
        /// This is a read-only operation intended for administrative or listing purposes.
        /// </remarks>
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

        /// <summary>
        /// Creates a new player account.
        /// </summary>
        /// <param name="request">
        /// The request containing the new player's registration data.
        /// </param>
        /// <returns>
        /// A <see cref="PlayerResponse"/> representing the newly created player.
        /// </returns>
        /// <remarks>
        /// - Initializes player statistics with default values.
        /// - Persists the new player to the repository.
        /// </remarks>
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

        /// <summary>
        /// Updates the password of an existing player.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the player whose password is being updated.
        /// </param>
        /// <param name="request">
        /// The request containing the old and new passwords.
        /// </param>
        /// <returns>
        /// An <see cref="UpdatePasswordResponse"/> if the update succeeds; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// - Validates the player's existence.
        /// - Ensures the provided old password matches the current password.
        /// - Persists the updated password upon successful validation.
        /// </remarks>
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


        /// <summary>
        /// Updates the last login timestamp of a player.
        /// </summary>
        /// <param name="playerId">
        /// The unique identifier of the player.
        /// </param>
        /// <remarks>
        /// This method is typically invoked after a successful login.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified player does not exist.
        /// </exception>
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

        /// <summary>
        /// Deletes a player from the system.
        /// </summary>
        /// <param name="playerId">
        /// The unique identifier of the player to delete.
        /// </param>
        /// <remarks>
        /// Permanently removes the player and all associated data from the repository.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when the specified player does not exist.
        /// </exception>
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

        public Player? GetPlayerById(int playerId)
        {
            return _repository.GetAllPlayers().FirstOrDefault(p => p.Id == playerId);
        }

        public string GenerateJwtToken(Player player)
        {
            return _tokenService.GenerateToken(player);
        }
    }

}
