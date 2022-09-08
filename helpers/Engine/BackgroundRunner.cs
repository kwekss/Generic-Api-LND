using helpers.Interfaces;
using helpers.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace helpers.Engine
{
    public class BackgroundRunner
    {
        private readonly List<IBackgroundJob> _jobs;
        private readonly int _delayTime;
        private readonly string _serverId;
        static readonly CancellationTokenSource _cancelationTokensSource = new CancellationTokenSource();
        public BackgroundRunner(IConfiguration config, IServiceProvider services)
        {
            AssemblyLoadContext.Default.Unloading += OnUnloadingEventHandler;

            _delayTime = Convert.ToInt32(config["BACKGROUND_SERVICE:LOOP_DELAY_SEC"] ?? "5");
            _serverId = config["BACKGROUND_SERVICE:SERVER_ID"];
            int jobInstances = Convert.ToInt32(config["BACKGROUND_SERVICE:JOB_INSTANCES"] ?? "1");
            bool isBackgrounRunnerEnabled = Convert.ToBoolean(config["BACKGROUND_SERVICE:ENABLED"] ?? "false");

            if (isBackgrounRunnerEnabled)
            {
                Event.Dispatch("log", $"Background service was started @ {DateTime.Now:yyyy-MM-dd hh:ss:mm tt}");
                _jobs = services.GetServices<IBackgroundJob>().ToList();
                RunJobs(jobInstances, TimeSpan.FromSeconds(_delayTime), _cancelationTokensSource);
            }

        }

        public void RunJobs(int instances, TimeSpan loopInterval, CancellationTokenSource cancellationTokenSource)
        {
            var serverId = GenerateServerId();
            for (int i = 0; i < instances; i++)
            {
                for (int j = 0; j < _jobs.Count; j++)
                {
                    var job = _jobs[j];
                    job.ServerId = serverId;
                    job.ThreadId = $"{i + 1}";
                    _ = StartJobs(job, loopInterval, cancellationTokenSource);
                }
            }
        }
        private async Task StartJobs(IBackgroundJob job, TimeSpan loopInterval, CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"{job.GetType().Name} => Server ID: {job.ServerId}; ThreadID: {job.ThreadId}; Heartbeat @ {DateTime.Now:yyyy-MM-dd hh:ss:mm tt}");
                    await job.RunJob();
                }
                catch (Exception e)
                {
                    Event.Dispatch("log", e.ToString());
                }
                await Task.Delay(loopInterval, cancellationTokenSource.Token);
            }
        }
        public string GenerateServerId()
        {
            var serverId = _serverId;
            return string.IsNullOrWhiteSpace(serverId) ? Guid.NewGuid().ToString().Split("-").FirstOrDefault() : serverId;
        }

        private static void OnUnloadingEventHandler(AssemblyLoadContext obj)
        {
            Console.WriteLine($"Background service was stopped on OnUnloadingEventHandler");
            Event.Dispatch("log", $"Background service was stopped on OnUnloadingEventHandler @ {DateTime.Now:yyyy-MM-dd hh:ss:mm tt}");
        }
    }
}
