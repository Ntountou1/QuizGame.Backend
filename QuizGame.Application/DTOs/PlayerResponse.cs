using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.DTOs
{
    public class PlayerResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogInAt { get; set; }
        public int TotalScore { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int CurrentRank { get; set; }
        public int Level { get; set; }
    }
}
