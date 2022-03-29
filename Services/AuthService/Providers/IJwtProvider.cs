using AuthService.Models;

namespace AuthService.Providers
{
    public interface IJwtProvider
    {
        JwtInfo CreateJwtInfo(string username, string roles);
    }
}