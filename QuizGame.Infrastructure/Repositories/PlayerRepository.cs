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

        /// <summary>
        /// Retrieves all players from the storage file.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="Player"/> objects.
        /// Returns an empty collection if the file does not exist or no players are stored.
        /// </returns>
        /// <remarks>
        /// - Reads the JSON file at <c>_filePath</c> and deserializes it.
        /// - Does not modify any data.
        /// </remarks>
        public IEnumerable<Player> GetAllPlayers() {
            if (!File.Exists(_filePath)) 
                return Enumerable.Empty<Player>();

            var json = File.ReadAllText(_filePath);
            var players = JsonSerializer.Deserialize<List<Player>>(json);

            return players ?? Enumerable.Empty<Player>();
        }

        /// <summary>
        /// Adds a new player to the storage file.
        /// </summary>
        /// <param name="newPlayer">The <see cref="Player"/> object to add.</param>
        /// <remarks>
        /// - Assigns a new unique ID to the player.
        /// - Reads the existing JSON file and appends the new player.
        /// - If the file does not exist, creates a new collection.
        /// - Persists the updated list of players to the JSON file.
        /// </remarks>
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

        /// <summary>
        /// Updates an existing player's information in the storage file.
        /// </summary>
        /// <param name="updatedPlayer">The <see cref="Player"/> object containing updated data.</param>
        /// <remarks>
        /// - Finds the player by ID and replaces the existing record.
        /// - If the player is not found, the file remains unchanged.
        /// - Persists the updated list of players to the JSON file.
        /// </remarks>
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

        /// <summary>
        /// Deletes a player from the storage file by their ID.
        /// </summary>
        /// <param name="id">The unique identifier of the player to delete.</param>
        /// <remarks>
        /// - Reads the current player list from the JSON file.
        /// - Removes the player if found; otherwise, does nothing.
        /// - Persists the updated list back to the file.
        /// </remarks>
        public void DeletePlayer(int id)
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

            // Remove
            var existing = players.FirstOrDefault(p => p.Id == id);
            if (existing != null)
            {
                players.Remove(existing);

                var updatedJson = JsonSerializer.Serialize(players, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_filePath, updatedJson);
            }
        }

        /// <summary>
        /// Retrieves a single player by their unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the player.</param>
        /// <returns>
        /// The <see cref="Player"/> object if found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// - Reads the JSON file at <c>_filePath</c> and searches for the player.
        /// - Does not modify any data.
        /// </remarks>
        public Player? GetPlayerById(int id)
        {
            if (!File.Exists(_filePath))
                return null;

            var json = File.ReadAllText(_filePath);
            var players = JsonSerializer.Deserialize<List<Player>>(json) ?? new List<Player>();

            return players.FirstOrDefault(p => p.Id == id);
        }

    }
}
