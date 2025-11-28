using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Domain.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public string Role { get; set; } = "User";
        public string? Username { get; set; }
        public string? Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogInAt { get; set; }
        public int TotalScore { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int CurrentRank { get; set; }
        public int Level { get; set; }
    }
}
