using System.Threading.Tasks;

namespace helpers.Notifications
{
    public interface ISmsNotification
    {
        Task<string> Dispatch(string recipient, string content, params dynamic[] extra);
    }
}