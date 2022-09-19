using helpers.Interfaces;
using helpers.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
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
        private readonly List<IAutoRun> _autoRunners;
        private readonly int _delayTime;
        private readonly string _serverId;
        static readonly CancellationTokenSource _cancelationTokensSource = new CancellationTokenSource();

        private readonly IMessengerHub _messengerHub;

        public BackgroundRunner(IMessengerHub messengerHub, IConfiguration config, IServiceProvider services)
        {
            AssemblyLoadContext.Default.Unloading += OnUnloadingEventHandler;
            _messengerHub = messengerHub;

            _delayTime = config.GetValue("BACKGROUND_SERVICE:LOOP_DELAY_SEC", 5);
            _serverId = config.GetValue("BACKGROUND_SERVICE:SERVER_ID", "");
            int jobInstances = config.GetValue("BACKGROUND_SERVICE:JOB_INSTANCES", 1);
            bool isBackgrounRunnerEnabled = config.GetValue("BACKGROUND_SERVICE:ENABLED", false);

            _autoRunners = services.GetServices<IAutoRun>().ToList();
            ExecuteAutoRun();

            if (isBackgrounRunnerEnabled)
            {
                Log.ForContext("CorrelationId", "BackgroundService").Information($"Background service was started");
                _jobs = services.GetServices<IBackgroundJob>().ToList();
                RunJobs(jobInstances, TimeSpan.FromSeconds(_delayTime), _cancelationTokensSource);
            }

        }

        private void ExecuteAutoRun()
        {
            if (_autoRunners == null || !_autoRunners.Any()) return;

            for (int j = 0; j < _autoRunners.Count; j++)
            {
                var job = _autoRunners[j];
                _ = job.Start();
            }
        }

        private void RunJobs(int instances, TimeSpan loopInterval, CancellationTokenSource cancellationTokenSource)
        {
            if (_jobs == null || !_jobs.Any()) return;

            var serverId = GenerateServerId();

            for (int i = 0; i < instances; i++)
            {
                for (int j = 0; j < _jobs.Count; j++)
                {
                    var job = _jobs[j];
                    LogContext.PushProperty("CorrelationId", job.GetType().Name);
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
                    Log.Error(e.ToString());
                }
                await Task.Delay(loopInterval, cancellationTokenSource.Token);
            }
        }
        public string GenerateServerId()
        {
            var serverId = _serverId;
            return string.IsNullOrWhiteSpace(serverId) ? Guid.NewGuid().ToString().Split("-").FirstOrDefault() : serverId;
        }

        private void OnUnloadingEventHandler(AssemblyLoadContext obj)
        {
            _messengerHub.Publish(new LogWriter("info", $"Background service was stopped on OnUnloadingEventHandler"));
        }
    }
}
