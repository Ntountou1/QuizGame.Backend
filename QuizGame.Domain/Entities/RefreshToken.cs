using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGame.Domain.Entities
{
    public class RefreshToken
    {
        public string Token { get; set; } = null!;
        public int PlayerId { get; set; }
        public DateTime Expires { get; set; }
        public bool Revoked { get; set; } = false;
    }
}
