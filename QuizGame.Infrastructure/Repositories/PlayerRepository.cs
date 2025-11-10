using QuizGame.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using QuizGame.Domain.Interfaces;

namespace QuizGame.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly string _filePath;

        public PlayerRepository()
        {
            // Points to the Data folder inside output directory
            _filePath = @"C:\Users\Panagiotis\Desktop\QuizGame\players.json";
        }

        public IEnumerable<Player> GetAllPlayers() {
            if (!File.Exists(_filePath)) 
                return Enumerable.Empty<Player>();

            var json = File.ReadAllText(_filePath);
            var players = JsonSerializer.Deserialize<List<Player>>(json);

            return players ?? Enumerable.Empty<Player>();
        }

       public void AddPlayer(Player newPlayer)
        {
            List<Player> players;

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                players = JsonSerializer.Deserialize<List<Player>>(json) ?? new List<Player>();
            }
            else
            {
                players = new List<Player>();
            }

            int nextId = players.Any() ? players.Max(p => p.Id) + 1 : 1;
            newPlayer.Id = nextId;

            players.Add(newPlayer);

            var updatedJson = JsonSerializer.Serialize(players, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, updatedJson); 
        }

        public void UpdatePlayer(Player updatedPlayer)
        {
            List<Player> players;
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                players = JsonSerializer.Deserialize<List<Player>>(json) ?? new List<Player>();
            }
            else
            {
                players = new List<Player>();
            }

            var index = players.FindIndex(p => p.Id == updatedPlayer.Id);
            if (index != -1)
            {
                players[index] = updatedPlayer;
                var updatedJson = JsonSerializer.Serialize(players, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, updatedJson);
            }
        }
    }
}
