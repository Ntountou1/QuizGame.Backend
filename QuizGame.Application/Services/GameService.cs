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

        /// <summary>
        /// Starts a new quiz game session for the specified player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player starting the game.</param>
        /// <returns>
        /// A <see cref="StartGameResponse"/> containing session metadata and the selected question IDs.
        /// </returns>
        /// <remarks>
        /// - Randomly selects a fixed set of questions based on difficulty.
        /// - Initializes a new game session with status <c>InProgress</c>.
        /// - Throws an exception if there are insufficient questions available.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no questions or not enough questions of the required difficulty exist.
        /// </exception>
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

        /// <summary>
        /// Retrieves a game session by its identifier and returns it as a read-only DTO.
        /// </summary>
        /// <param name="gameSessionId">The unique identifier of the game session.</param>
        /// <returns>
        /// A <see cref="QuizGame.Application.DTOs.GameSession"/> representing the current state
        /// of the requested game session.
        /// </returns>
        /// <remarks>
        /// - This method is intended for query/read-only scenarios.
        /// - The returned object is a DTO and does not allow mutation of session state.
        /// - Domain-specific properties are mapped to a transport-friendly format.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the requested game session does not exist.
        /// </exception>
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

        /// <summary>
        /// Submits an answer for a specific question within an active game session.
        /// </summary>
        /// <param name="request">
        /// The request containing the game session ID, question ID, and selected answer ID.
        /// </param>
        /// <returns>
        /// A <see cref="SubmitAnswerResponse"/> indicating correctness, points earned,
        /// total session score, and whether the game has completed.
        /// </returns>
        /// <remarks>
        /// - Validates session and question state.
        /// - Evaluates the submitted answer and updates session score.
        /// - Tracks time taken per question.
        /// - Completes the session and updates player statistics when all questions are answered.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the game session, question, or player cannot be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the session is not active or the question has already been answered.
        /// </exception>
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

        /// <summary>
        /// Retrieves an active game session by ID.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the game session.</param>
        /// <returns>
        /// The active <see cref="GameSession"/> domain entity.
        /// </returns>
        /// <remarks>
        /// This method enforces that the session exists and is currently in progress.
        /// It is intended for command-side operations that mutate session state.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the session does not exist.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the session is not in the <c>InProgress</c> state.
        /// </exception>
        private GameSession GetActiveSession(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
                throw new KeyNotFoundException("Game session not found");

            if (session.Status != GameSessionStatus.InProgress)
                throw new InvalidOperationException("Game session is not active");

            return session;
        }

        /// <summary>
        /// Retrieves an unanswered question belonging to a specific game session.
        /// </summary>
        /// <param name="session">The game session containing the question.</param>
        /// <param name="questionId">The unique identifier of the question.</param>
        /// <returns>
        /// The <see cref="GameQuestion"/> associated with the session.
        /// </returns>
        /// <remarks>
        /// Ensures that the question exists within the session and has not already been answered.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the question does not belong to the session.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the question has already been answered.
        /// </exception>
        private GameQuestion GetUnansweredQuestion(GameSession session, int questionId)
        {
            var question = session.QuestionIds.FirstOrDefault(q => q.QuestionId == questionId);
            if (question == null)
                throw new KeyNotFoundException("Question not found in this session");

            if (question.TimeTaken.HasValue)
                throw new InvalidOperationException("This question has already been answered");

            return question;
        }

        /// <summary>
        /// Evaluates whether a submitted answer is correct and calculates the awarded points.
        /// </summary>
        /// <param name="questionId">The unique identifier of the question.</param>
        /// <param name="answerId">The identifier of the submitted answer.</param>
        /// <returns>
        /// A tuple containing a boolean indicating correctness and the points awarded.
        /// </returns>
        /// <remarks>
        /// Retrieves the authoritative question data from the repository to validate the answer.
        /// </remarks>
        private (bool isCorrect, int points) EvaluateAnswer(int questionId, int answerId)
        {
            var questionData = _questionRepository
                .GetAllQuestions()
                .First(q => q.Id == questionId);

            bool isCorrect = answerId == questionData.CorrectAnswerId;
            int points = isCorrect ? questionData.Points : 0;

            return (isCorrect, points);
        }

        /// <summary>
        /// Determines whether all questions in a game session have been answered.
        /// </summary>
        /// <param name="session">The game session to evaluate.</param>
        /// <returns>
        /// <c>true</c> if all questions have a recorded answer time; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// A question is considered answered when its <c>TimeTaken</c> value is set.
        /// </remarks>
        private bool IsSessionCompleted(GameSession session)
        {
            return session.QuestionIds.All(q =>
                q.TimeTaken.HasValue && q.TimeTaken.Value > TimeSpan.Zero);
        }

        /// <summary>
        /// Marks a game session as completed and sets its end time.
        /// </summary>
        /// <param name="session">The game session to complete.</param>
        /// <remarks>
        /// This method performs only session state finalization.
        /// Player-related updates are handled elsewhere.
        /// </remarks>
        private void CompleteSession(GameSession session)
        {
            session.Status = GameSessionStatus.Completed;
            session.EndTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Recalculates and updates the CurrentRank of all players based on their TotalScore.
        /// The player with the highest TotalScore receives rank 1, the next highest rank 2, and so on.
        /// </summary>
        /// <remarks>
        /// This method should be called after any update to a player's TotalScore to ensure ranks are up to date.
        /// It loads all players from the PlayerRepository, sorts them in descending order of TotalScore,
        /// assigns ranks sequentially, and updates each player back to the repository.
        /// </remarks>
        private void UpdatePlayerRanks()
        {
            var players = _playerRepository.GetAllPlayers().ToList();
            var sortedPlayers = players.OrderByDescending(p => p.TotalScore).ToList();

            int rank = 1;
            foreach (var player in sortedPlayers)
            {
                player.CurrentRank = rank++;
                _playerRepository.UpdatePlayer(player);
            }
        }

        /// <summary>
        /// Updates aggregate player statistics after a completed game session.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="score">The score achieved in the completed session.</param>
        /// <remarks>
        /// - Increments total score by the session score.
        /// - Increments the number of games played.
        /// - Persists the updated player state.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the player cannot be found.
        /// </exception>
        private void UpdatePlayerStats(int playerId, int score)
        {
            var player = _playerRepository.GetPlayerById(playerId);
            if (player == null)
                throw new KeyNotFoundException("Player not found");

            player.TotalScore += score;
            player.GamesPlayed += 1;

            //See current level of the Player by the repository layer
            player.Level = _playerRepository.GetLevelForScore(player.TotalScore);
            _playerRepository.UpdatePlayer(player);

            // Update ranks for all players
            UpdatePlayerRanks();
        }
    }
}
