using helpers.Interfaces;
using models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundService.Jobs
{
    public class TaskScheduler : ITaskScheduler
    {
        public TaskScheduler()
        {

        }

        public async Task<IEnumerable<dynamic>> FetchQueue(string serverId, string threadId, int limit)
        {
            var queue = new List<TestQueue>();
            // Fetch new messages from the queue and add them to messageQueue
            // Replace this with your actual queue fetching logic
            for (int i = 0; i < limit; i++) // Simulated queue items
            {
                queue.Add(new TestQueue { QueueId = i, TestProp = $"prop {DateTime.Now}" });
            }

            return queue;
        }

        public async Task<TaskCompletedResponse> RunTask(dynamic item)
        {
            await Task.Delay(1000);
            return new TaskCompletedResponse { Success = true, Item = item };
        }

        public async Task TaskCompleted(TaskCompletedResponse response)
        {
 
        } 
    }

    public class TestQueue
    {
        public int QueueId { get; set; }
        public string TestProp { get; set; }
    }
}
