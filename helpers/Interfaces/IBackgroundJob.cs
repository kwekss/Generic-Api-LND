using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public interface IBackgroundJob
    {
        Task RunJob();
        string ThreadId { get; set; }
        string ServerId { get; set; }
    }
}