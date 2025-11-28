using QuizGame.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Domain.Interfaces
{
    public  interface IQuestionRepository
    {
        IEnumerable<Question> GetAllQuestions();
    }
}
