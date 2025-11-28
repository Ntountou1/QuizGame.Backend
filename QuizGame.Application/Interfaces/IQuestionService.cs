using QuizGame.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.Interfaces
{
    public interface IQuestionService
    {
        IEnumerable<QuestionResponse> GetAllQuestions();
    }
}
