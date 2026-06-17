
namespace MusicAggregator.Application.Auth
{
    public interface ITokenService
    {
        string CreateToken(string username);
    }
}
