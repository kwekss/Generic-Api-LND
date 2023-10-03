using helpers.Interfaces;
using helpers.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using models;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace helpers.Engine.Scheduler
{
    public class TaskScheduleEngine
    {

        private readonly List<ITaskScheduler> _tasks;
        private readonly string _serverId;
        private readonly int _item_per_processing;
        static readonly CancellationTokenSource _cancelationTokensSource = new CancellationTokenSource();
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly List<ScheduleConfig> _scheduleConfig;

        public TaskScheduleEngine(ISchedulerProvider schedulerProvider, IConfiguration config, IServiceProvider services)
        {
            _schedulerProvider = schedulerProvider;

            _scheduleConfig = config.GetSection("BACKGROUND_SERVICE:SCHEDULE_CONFIG").Get<List<ScheduleConfig>>().Where(x => x.Active)?.ToList();

            _serverId = config.GetValue("BACKGROUND_SERVICE:SERVER_ID", Guid.NewGuid().ToString().Split("-").FirstOrDefault());

            int jobInstances = config.GetValue("BACKGROUND_SERVICE:JOB_INSTANCES", 1);
            _item_per_processing = config.GetValue("BACKGROUND_SERVICE:ITEM_PER_PROCESSING", 1);

            if (_scheduleConfig != null && _scheduleConfig.Any())
            {
                AssemblyLoadContext.Default.Unloading += OnUnloadingEventHandler;
                Log.ForContext("CorrelationId", "TaskSchedulerService").Information($"Task Scheduler service was started");
                _tasks = services.GetServices<ITaskScheduler>().ToList();

                RunTasks(jobInstances, _cancelationTokensSource);
            }

        }
        private void RunTasks(int instances, CancellationTokenSource cancellationTokenSource)
        {
            if (_tasks == null || !_tasks.Any()) return;

            var serverId = GenerateServerId();

            for (int i = 0; i < instances; i++)
            {
                for (int j = 0; j < _tasks.Count; j++)
                {
                    var task = _tasks[j];
                    Log.Information($"Running Task: {task.GetType().Name}");

                    for (int k = 0; k < _scheduleConfig.Count; k++)
                    {
                        var runner = new TaskRunner(task, cancellationTokenSource.Token, _item_per_processing);
                        var period = _scheduleConfig[k];
                        if (period.Type == "SECONDS")
                            _schedulerProvider.IntervalInSeconds(period.StartHour, period.StartMinute, period.Interval, () => { runner.Execute(period, serverId, $"{i + 1}").Wait(); });

                        if (period.Type == "MINUTES")
                            _schedulerProvider.IntervalInMinutes(period.StartHour, period.StartMinute, period.Interval, () => { runner.Execute(period, serverId, $"{i + 1}").Wait(); });

                        if (period.Type == "HOURS")
                            _schedulerProvider.IntervalInDays(period.StartHour, period.StartMinute, period.Interval, () => { runner.Execute(period, serverId, $"{i + 1}").Wait(); });

                        if (period.Type == "DAILY")
                            _schedulerProvider.IntervalInDays(period.StartHour, period.StartMinute, period.Interval, () => { runner.Execute(period, serverId, $"{i + 1}").Wait(); });
                    }
                }
            }
        }

        public string GenerateServerId()
        {
            var serverId = _serverId;
            return string.IsNullOrWhiteSpace(serverId) ? Guid.NewGuid().ToString().Split("-").FirstOrDefault() : serverId;
        }

        private void OnUnloadingEventHandler(AssemblyLoadContext obj)
        {
            Log.ForContext("CorrelationId", "BackgroundService").Information($"Background service was stopped on OnUnloadingEventHandler");
        }
    }

    public class TaskRunner
    {
        private static Queue<object> _queue = new Queue<object>();
        private readonly ITaskScheduler _taskScheduler;
        private readonly CancellationToken _cancellationToken;
        private readonly int _item_per_processing;
        private static bool _isBusy;
        private static DateTime _processedTimestamp;

        public TaskRunner(ITaskScheduler taskScheduler, CancellationToken cancellationToken, int item_per_processing)
        {
            _taskScheduler = taskScheduler;
            _cancellationToken = cancellationToken;
            _item_per_processing = item_per_processing; 
        }

        public async Task Execute(ScheduleConfig config, string serverId, string threadId, int defaultBatchSize = 100)
        {
            try
            {
                Logger($"Pending in queue: {_queue.Count()}, Is Busy: {_isBusy}, Last Processed at: {_processedTimestamp:yyyy-MM-dd hh:mm:ss tt}");

                if (_queue.Count < defaultBatchSize)
                {
                    var limit = defaultBatchSize - _queue.Count();
                    if (limit <= 0) return;

                    var queue = await _taskScheduler.FetchQueue(serverId, threadId, limit);
                    if (queue != null && queue.Any())
                    {
                        Logger($"New queue items fetched: {queue.Count()}");
                        foreach (dynamic item in queue)
                        {
                            _queue.Enqueue(item);
                        }
                    }
                }

                if (!_isBusy) LockAndExecute();
            }
            catch (Exception e)
            {
                Log.Error($"[{_taskScheduler.GetType().Name}] {e}");
            }

        } 

        private async Task ProcessSingle(object item)
        {
            Logger($"Processing item: {item.Stringify()}");
            _isBusy = true;

            TaskCompletedResponse response = await _taskScheduler.RunTask(item);
            _processedTimestamp = DateTime.Now;
            Logger($"Processed item: {item.Stringify()}, Pending processing: {_queue.Count()}");

            await _taskScheduler.TaskCompleted(response);
            _isBusy = false;
        }

        private void LockAndExecute()
        {
            CountdownEvent countdownEvent = new CountdownEvent(_item_per_processing);
            for (int i = 0; i < _item_per_processing; i++)
            {
                _ = Task.Run(() =>
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        object item = null;

                        // Dequeue an item inside a lock to ensure thread safety
                        lock (_queue)
                        {
                            if (_queue.Count > 0)
                            {
                                item = _queue.Dequeue();
                                if (item != null)
                                {
                                    _isBusy = true;
                                    ProcessSingle(item).Wait();
                                }
                            }
                        }

                        if (item == null)
                        {
                            // No more items in the queue
                            break;
                        }

                        // Signal that the task is done processing an item
                        if (countdownEvent.CurrentCount > 0) countdownEvent.Signal();
                    }
                });
            }

            // Wait for all tasks to complete
            countdownEvent.Wait();
            _isBusy = false;
        }
        private void Logger(string message)
        {
            Console.WriteLine($"[{_taskScheduler.GetType().Name}] {message}");
            //Log.Information($"[{_taskScheduler.GetType().Name}] {message}");
        }
    }
}
