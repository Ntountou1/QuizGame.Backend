using QuizGame.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Domain.Interfaces
{
    public interface IPlayerRepository
    {
        IEnumerable<Player> GetAllPlayers();
        void AddPlayer(Player newPlayer);
        void UpdatePlayer(Player updatedPlayer);
        void DeletePlayer(int playerId);
        Player? GetPlayerById(int id);
    }
}
