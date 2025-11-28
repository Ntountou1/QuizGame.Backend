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
    public class QuestionService : IQuestionService
    {
        private readonly ILogger<QuestionService> _logger;
        private readonly IQuestionRepository _repository;

        public QuestionService(IQuestionRepository repository, ILogger<QuestionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public IEnumerable<QuestionResponse> GetAllQuestions()
        {
            _logger.LogInformation("Fetching all questions from repository.");

            var questions = _repository.GetAllQuestions();

            var response = questions.Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Category = q.Category,
                Difficulty = q.Difficulty,
                Points = q.Points,
                Answers = q.Answers,
                CorrectAnswerId = q.CorrectAnswerId
            });

            _logger.LogInformation("Retrieved {Count} questions.", response.Count());
            return response;
        }
    }
}
