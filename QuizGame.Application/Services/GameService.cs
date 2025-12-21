using Microsoft.Extensions.Logging;
using QuizGame.Application.DTOs;
using QuizGame.Application.Interfaces;
using QuizGame.Domain.Entities;
using QuizGame.Domain.Enums;
using QuizGame.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSession = QuizGame.Domain.Entities.GameSession;

namespace QuizGame.Application.Services
{
    public class GameService : IGameService
    {
        private readonly ILogger<GameService> _logger;
        private readonly IQuestionRepository _questionRepository;
        private readonly IPlayerRepository _playerRepository;

        // Temporary in-memory storage for sessions
        private static readonly List<GameSession> _sessions = new();

        public GameService(IQuestionRepository questionRepository, ILogger<GameService> logger, IPlayerRepository playerRepository)
        {
            _questionRepository = questionRepository;
            _logger = logger;
            _playerRepository = playerRepository;
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
                Status = GameSessionStatus.InProgress,
                TotalQuestions = 5,
                TimeLimitSeconds = 10,
                QuestionIds = selectedQuestions.Select(q => new GameQuestion { 
                    QuestionId = q.Id,
                    QuestionStartTime = DateTime.UtcNow
                }).ToList()
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
                QuestionIds = newSession.QuestionIds.Select(q => q.QuestionId)
            };

        }

        public QuizGame.Application.DTOs.GameSession GetGameSession(int gameSessionId)
        {
            _logger.LogInformation("Fetching game session {SessionId}", gameSessionId);
            var session = _sessions.FirstOrDefault(s => s.Id == gameSessionId);
            if (session == null)
            {
                _logger.LogWarning("Game session {SessionId} not found", gameSessionId);
                throw new KeyNotFoundException($"Game session {gameSessionId} not found");
            }

            // Map domain model to DTO
            var dto = new QuizGame.Application.DTOs.GameSession
            {
                Id = session.Id,
                PlayerId = session.PlayerId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Score = session.Score,
                Status = session.Status.ToString(),
                TotalQuestions = session.TotalQuestions,
                TimeLimitSeconds = session.TimeLimitSeconds,
                QuestionIds = session.QuestionIds.Select(q => q.QuestionId)
            };

            return dto;
        }

        public SubmitAnswerResponse SubmitAnswer(SubmitAnswerRequest request)
        {
            var session = GetActiveSession(request.GameSessionId);
            var question = GetUnansweredQuestion(session, request.QuestionId);

            var (isCorrect, points) = EvaluateAnswer(request.QuestionId, request.AnswerId);

            session.Score += points;
            question.TimeTaken = DateTime.UtcNow - question.QuestionStartTime;

            bool isCompleted = IsSessionCompleted(session);

            if (isCompleted)
            {
                CompleteSession(session);
                UpdatePlayerStats(session.PlayerId, session.Score);
            }

            return new SubmitAnswerResponse
            {
                IsCorrect = isCorrect,
                PointsEarned = points,
                TotalScore = session.Score,
                IsGameCompleted = isCompleted
            };
        }


        private GameSession GetActiveSession(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
                throw new KeyNotFoundException("Game session not found");

            if (session.Status != GameSessionStatus.InProgress)
                throw new InvalidOperationException("Game session is not active");

            return session;
        }

        private GameQuestion GetUnansweredQuestion(GameSession session, int questionId)
        {
            var question = session.QuestionIds.FirstOrDefault(q => q.QuestionId == questionId);
            if (question == null)
                throw new KeyNotFoundException("Question not found in this session");

            if (question.TimeTaken.HasValue)
                throw new InvalidOperationException("This question has already been answered");

            return question;
        }

        private (bool isCorrect, int points) EvaluateAnswer(int questionId, int answerId)
        {
            var questionData = _questionRepository
                .GetAllQuestions()
                .First(q => q.Id == questionId);

            bool isCorrect = answerId == questionData.CorrectAnswerId;
            int points = isCorrect ? questionData.Points : 0;

            return (isCorrect, points);
        }

        private bool IsSessionCompleted(GameSession session)
        {
            return session.QuestionIds.All(q =>
                q.TimeTaken.HasValue && q.TimeTaken.Value > TimeSpan.Zero);
        }

        private void CompleteSession(GameSession session)
        {
            session.Status = GameSessionStatus.Completed;
            session.EndTime = DateTime.UtcNow;
        }

        private void UpdatePlayerStats(int playerId, int score)
        {
            var player = _playerRepository.GetPlayerById(playerId);
            if (player == null)
                throw new KeyNotFoundException("Player not found");

            player.TotalScore += score;
            player.GamesPlayed += 1;

            _playerRepository.UpdatePlayer(player);
        }
    }
}
