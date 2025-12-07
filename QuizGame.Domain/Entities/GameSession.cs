using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizGame.Domain.Enums;

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
            public GameSessionStatus Status { get; set; } = GameSessionStatus.InProgress;
            public int TotalQuestions { get; set; } = 5;
            public int TimeLimitSeconds { get; set; } = 10;

            public List<GameQuestion> QuestionIds { get; set; } = new List<GameQuestion>();
            public TimeSpan? TotalTimeTaken { get; set; }
     }
}

