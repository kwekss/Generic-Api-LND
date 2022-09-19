using helpers.Interfaces;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BackgroundService.Jobs
{
    public class TestBackgroundWorker : IBackgroundJob
    {
        public string ThreadId { get; set; }
        public string ServerId { get; set; }

        public TestBackgroundWorker()
        {

        }
        public async Task RunJob()
        {

            Log.Error($"{nameof(TestBackgroundWorker)} is reachable");
            throw new Exception();
        }
    }
}
