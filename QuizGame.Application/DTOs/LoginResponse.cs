using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Application.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int UserId { get; set; }
    }
}
