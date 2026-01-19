using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using QuizGame.Domain.Entities;

namespace QuizGame.Infrastructure.Repositories
{
    public class RefreshTokenService
    {
        private readonly List<RefreshToken> _tokens = new();


        /// <summary>
        /// Generates a secure refresh token for the specified player.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>The generated <see cref="RefreshToken"/>.</returns>
        public RefreshToken GenerateRefreshToken (int playerId)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshToken = new RefreshToken
            {
                Token = token,
                PlayerId = playerId,
                Expires = DateTime.UtcNow.AddDays(7), //long-lived token
                Revoked = false
            };

            _tokens.Add(refreshToken);
            return refreshToken;
        }


        /// <summary>
        /// Validates the refresh token.
        /// </summary>
        /// <param name="token">The refresh token string.</param>
        /// <returns>The <see cref="RefreshToken"/> if valid; otherwise null.</returns>
        public RefreshToken? ValidateRefreshToken(string token)
        {
            var refreshToken = _tokens.FirstOrDefault(t => t.Token == token);
            if (refreshToken == null) return null;
            if (refreshToken.Revoked) return null;
            if (refreshToken.Expires <= DateTime.UtcNow) return null;

            return refreshToken;
        }

        /// <summary>
        /// Revokes the specified refresh token.
        /// </summary>
        /// <param name="token">The refresh token string.</param>
        public void RevokeRefreshToken(string token)
        {
            var refreshToken = _tokens.FirstOrDefault(x => x.Token == token);

            if (refreshToken != null)
            {
                refreshToken.Revoked = true;
            }
        }
    }
}
