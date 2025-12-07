using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Domain.Entities
{
    public class GameQuestion
    {
        public int QuestionId { get; set; }
        public int? SelectedAnswerId { get; set; }
        public bool IsCorrect { get; set; }
        public TimeSpan? TimeTaken { get; set; }
    }
}
