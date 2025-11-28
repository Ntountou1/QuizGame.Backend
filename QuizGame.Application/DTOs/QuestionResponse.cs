using QuizGame.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.DTOs
{
    public class QuestionResponse
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int Points { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
        public int CorrectAnswerId { get; set; }
    }
}
