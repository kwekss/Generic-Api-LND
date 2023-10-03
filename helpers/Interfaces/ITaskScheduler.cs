using models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public interface ITaskScheduler
    {
        Task<IEnumerable<dynamic>> FetchQueue(string serverId, string threadId, int limit);
        Task<TaskCompletedResponse> RunTask(dynamic item);
        Task TaskCompleted(TaskCompletedResponse response);
    }
}
