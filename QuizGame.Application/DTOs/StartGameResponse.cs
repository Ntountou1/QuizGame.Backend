using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.DTOs
{
    public class StartGameResponse
    {
        public int GameSessionId { get; set; }
        public int PlayerId { get; set; }
        public DateTime StartTime { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeLimitSeconds { get; set; }
        public IEnumerable<int> QuestionIds { get; set; } = Array.Empty<int>();
    }
}
