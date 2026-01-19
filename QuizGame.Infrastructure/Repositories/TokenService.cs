using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QuizGame.Domain.Entities;

namespace QuizGame.Infrastructure.Repositories
{
    public class TokenService
    {
        private readonly string _secretKey;

        public TokenService(string secretKey)
        {
            _secretKey = secretKey;
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified player.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> object for whom the token is generated.</param>
        /// <returns>
        /// A <see cref="string"/> representing the signed JWT.
        /// </returns>
        /// <remarks>
        /// - The token includes the player's ID and username as claims.
        /// - The token is signed using HMAC SHA-256 with a symmetric key.
        /// - The token expires 1 day after creation.
        /// - Intended for authentication and authorization purposes.
        /// </remarks>
        public string GenerateToken(Player player) {
            var tokenHandler = new JwtSecurityTokenHandler(); 
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                    new Claim(ClaimTypes.Name, player.Username ?? "")
                }
                ),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
