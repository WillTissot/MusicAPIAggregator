using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicAggregator.Application.Auth;
using MusicAggregator.Infrastructure.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MusicAggregator.Tests
{
    public class TokenServiceTests
    {
        private static TokenService CreateSut(out JwtOptions o)
        {
            o = new JwtOptions
            {
                Issuer = "MusicAggregator",
                Audience = "MusicAggregatorClients",
                Key = "test-signing-key-at-least-32-bytes-long!!",
                ExpiryMinutes = 60
            };
            return new TokenService(Options.Create(o));
        }

        [Fact]
        public void CreateToken_ProducesTokenThatValidates_WithExpectedClaims()
        {
            var sut = CreateSut(out var o);

            var jwt = sut.CreateToken("demo");

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = o.Issuer,
                ValidAudience = o.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key)),
                ClockSkew = TimeSpan.Zero
            };

            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(jwt, parameters, out var validated);

            Assert.Equal("demo", principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                               ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var token = Assert.IsType<JwtSecurityToken>(validated);
            Assert.Equal(o.Issuer, token.Issuer);
            Assert.Contains(o.Audience, token.Audiences);   // ◄── would have failed before your fix
        }

        [Fact]
        public void CreateToken_WrongKey_FailsValidation()
        {
            var sut = CreateSut(out var o);
            var jwt = sut.CreateToken("demo");

            var badParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("a-completely-different-32-byte-key!!!")),
                ValidIssuer = o.Issuer,
                ValidAudience = o.Audience
            };

            Assert.ThrowsAny<SecurityTokenException>(() =>
                new JwtSecurityTokenHandler().ValidateToken(jwt, badParams, out _));
        }
    }
}
