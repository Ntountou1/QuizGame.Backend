using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Domain.Entities
{
    public class GameSession
    {
            public int Id { get; set; }
            public int PlayerId { get; set; }
            public Player Player { get; set; } = null!;
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public int Score { get; set; } = 0;
            public string Status { get; set; } = "InProgress"; // InProgress, Completed, Abandoned
            public int TotalQuestions { get; set; } = 5;
            public int TimeLimitSeconds { get; set; } = 10;

            public List<int> QuestionIds { get; set; } = new List<int>();
            public TimeSpan? TotalTimeTaken { get; set; }
     }
}

