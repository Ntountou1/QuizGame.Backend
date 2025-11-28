using QuizGame.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.Interfaces
{
    public interface IPlayerService
    {
        IEnumerable<PlayerResponse> GetPlayers();
        PlayerResponse CreatePlayer(CreatePlayerRequest request);
        LoginResponse? Login(LoginRequest request);
        UpdatePasswordResponse? UpdatePassword(int userId, UpdatePasswordRequest request);
        void UpdateLastLogin(int playerId);
    }
}
