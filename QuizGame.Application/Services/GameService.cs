using Microsoft.Extensions.Logging;
using QuizGame.Application.DTOs;
using QuizGame.Application.Interfaces;
using QuizGame.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.Services
{
    public class GameService : IGameService
    {
        private readonly ILogger<GameService> _logger;
        private readonly IQuestionRepository _questionRepository;

        // Temporary in-memory storage for sessions
        private static readonly List<GameSession> _sessions = new();

        public GameService(IQuestionRepository questionRepository, ILogger<GameService> logger)
        {
            _questionRepository = questionRepository;
            _logger = logger;
        }

        public StartGameResponse StartNewGame (int playerId)
        {
            _logger.LogInformation("Starting new game for player {PlayerId}", playerId);

            var allQuestions = _questionRepository.GetAllQuestions().ToList();

            if (!allQuestions.Any())
            {
                _logger.LogWarning("No questions available to start game");
                throw new InvalidOperationException("No questions available");
            }

            // Select 2 easy, 2 medium, 1 hard randomly
            var easy = allQuestions.Where(q => q.Difficulty.Equals("Easy", StringComparison.OrdinalIgnoreCase))
                                  .OrderBy(_ => Guid.NewGuid()).Take(2);
            var medium = allQuestions.Where(q => q.Difficulty.Equals("Medium", StringComparison.OrdinalIgnoreCase))
                                     .OrderBy(_ => Guid.NewGuid()).Take(2);
            var hard = allQuestions.Where(q => q.Difficulty.Equals("Hard", StringComparison.OrdinalIgnoreCase))
                                   .OrderBy(_ => Guid.NewGuid()).Take(1);

            var selectedQuestions = easy.Concat(medium).Concat(hard).ToList();

            if (selectedQuestions.Count < 5)
            {
                _logger.LogWarning("Not enough questions of required difficulty to start game");
                throw new InvalidOperationException("Not enough questions to start game");
            }

            var newSession = new GameSession
            {
                Id = _sessions.Count + 1,
                PlayerId = playerId,
                StartTime = DateTime.UtcNow,
                Score = 0,
                Status = "InProgress",
                TotalQuestions = 5,
                TimeLimitSeconds = 10,
                QuestionIds = selectedQuestions.Select(q => q.Id)
            };

            _sessions.Add(newSession);

            _logger.LogInformation("Game session {SessionId} created for player {PlayerId}", newSession.Id, playerId);

            return new StartGameResponse
            {
                GameSessionId = newSession.Id,
                PlayerId = newSession.PlayerId,
                StartTime = newSession.StartTime,
                TotalQuestions = newSession.TotalQuestions,
                TimeLimitSeconds = newSession.TimeLimitSeconds,
                QuestionIds = newSession.QuestionIds
            };

        }

        public GameSession GetGameSession(int gameSessionId)
        {
            _logger.LogInformation("Fetching game session {SessionId}", gameSessionId);
            var session = _sessions.FirstOrDefault(s => s.Id == gameSessionId);
            if (session == null)
            {
                _logger.LogWarning("Game session {SessionId} not found", gameSessionId);
                throw new KeyNotFoundException($"Game session {gameSessionId} not found");
            }

            return session;
        }
    }
}
