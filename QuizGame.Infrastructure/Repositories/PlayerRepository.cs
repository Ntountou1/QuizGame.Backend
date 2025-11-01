using QuizGame.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace QuizGame.Infrastructure.Repositories
{
    public class PlayerRepository
    {
        private readonly string _filePath;

        public PlayerRepository()
        {
            // Points to the Data folder inside output directory
            _filePath = Path.Combine(AppContext.BaseDirectory, "Data", "players.json");
        }

        public IEnumerable<Player> GetAllPlayers() {
            if (!File.Exists(_filePath)) 
                return Enumerable.Empty<Player>();

            var json = File.ReadAllText(_filePath);
            var players = JsonSerializer.Deserialize<List<Player>>(json);

            return players ?? Enumerable.Empty<Player>();
        }
    }
}
