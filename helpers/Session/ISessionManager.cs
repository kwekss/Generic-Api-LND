using models;
using System.Threading.Tasks;

namespace helpers.Session
{
    public interface ISessionManager
    {
        Task DeleteSessionData(string token);
        Task<AppUserSession> GetAuthorizedUser();
        Task<AppUserSession> GetCurrentUserSession();
        Task<AppUserSession> SetAuthorizedUser<T>(T userData);
    }
    public interface IRoleManager
    {
        Task<bool> IsAuthorized(string userId,string role);
    }
}