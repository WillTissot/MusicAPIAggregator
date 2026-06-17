using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicAggregator.Application.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MusicAggregator.Infrastructure.Auth
{
    public sealed class TokenService(IOptions<JwtOptions> opts) : ITokenService
    {
        private readonly JwtOptions _o = opts.Value;

        public string CreateToken(string username)
        {
            var claims = new[]
            {
              new Claim(JwtRegisteredClaimNames.Sub, username),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
          };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.Key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _o.Issuer,
                audience: _o.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_o.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
