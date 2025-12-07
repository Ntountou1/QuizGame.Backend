using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizGame.Application.DTOs;

namespace QuizGame.Application.Interfaces
{
    public interface IGameService
    {
        StartGameResponse StartNewGame(int playerId);
        GameSession GetGameSession(int gameSessionId);
        SubmitAnswerResponse SubmitAnswer(SubmitAnswerRequest request);
    }
}
