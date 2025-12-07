using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.DTOs
{
    public class SubmitAnswerResponse
    {
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public int TotalScore { get; set; }
        public bool IsGameCompleted { get; set; }
    }
}
