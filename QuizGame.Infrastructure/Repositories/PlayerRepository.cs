using QuizGame.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Infrastructure.Repositories
{
    public class PlayerRepository
    {
        private readonly List<Player> _players =
        [
            new() { Id = 1, Username = "Panos" },
            new() { Id = 2, Username = "Hara" }
        ];

        public IEnumerable<Player> GetAllPlayers() => _players;
    }
}
