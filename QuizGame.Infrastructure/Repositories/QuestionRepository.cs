using QuizGame.Domain.Entities;
using QuizGame.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuizGame.Infrastructure.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly string _filePath;

        public QuestionRepository()
        {
            // Points to the json file that questions are stored temporarily
            _filePath = @"C:\Users\Panagiotis\Desktop\QuizGame\questions.json";
        }

        public IEnumerable<Question> GetAllQuestions()
        {
            if (!File.Exists(_filePath))
            {
                return Enumerable.Empty<Question>();
            }

            var json = File.ReadAllText(_filePath);
            var questions = JsonSerializer.Deserialize<List<Question>>(json);
            return questions ?? Enumerable.Empty<Question>();
        }
    }
}
