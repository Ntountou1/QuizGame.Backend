using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.DTOs
{
    public class SubmitAnswerRequest
    {
        public int GameSessionId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }
}
