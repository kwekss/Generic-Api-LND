using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Notifications
{
    public interface ISmsNotification
    {
        Task<string> Dispatch(string recipient, string content, params dynamic[] extra);
        Task<string> Dispatch(List<(string recipient, string message)> data, params dynamic[] extra);
    }
}