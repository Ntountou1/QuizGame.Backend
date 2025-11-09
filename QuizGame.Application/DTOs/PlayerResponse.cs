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
    }
}
