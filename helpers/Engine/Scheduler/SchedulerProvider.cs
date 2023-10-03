using helpers.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace helpers.Scheduler
{
    public class SchedulerProvider : ISchedulerProvider
    {
        private List<Timer> timers = new List<Timer>();

        public SchedulerProvider() { }

        private void ScheduleTask(int hour, int min, double intervalInHour, string type, Action task)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, 0);
            if (now > firstRun) firstRun = GetFirstRun(type, firstRun);

            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero) timeToGo = TimeSpan.Zero;


            Log.Information($"Task Scheduled at {firstRun}");
            var timer = new Timer(x =>
            {
                task.Invoke();
                Log.Information($"Task run at {DateTime.Now}. Next at {DateTime.Now.AddHours(intervalInHour)}");
            }, null, timeToGo, TimeSpan.FromHours(intervalInHour));

            timers.Add(timer);
        }
        private DateTime GetFirstRun(string type, DateTime firstRun)
        {
            return type switch
            {
                "SECONDS" => DateTime.Now.AddSeconds(1),
                "MINUTES" => DateTime.Now.AddMinutes(1),
                "HOURS" => DateTime.Now.AddHours(1),
                "DAYS" => firstRun.AddDays(1),
                _ => firstRun.AddDays(1)
            };
        }
        public void IntervalInSeconds(int hour, int sec, double interval, Action task)
        {
            interval = interval / 3600;
            ScheduleTask(hour, sec, interval, "SECONDS", task);
        }

        public void IntervalInMinutes(int hour, int min, double interval, Action task)
        {
            interval = interval / 60;
            ScheduleTask(hour, min, interval, "MINUTES", task);
        }

        public void IntervalInHours(int hour, int min, double interval, Action task)
        {
            ScheduleTask(hour, min, interval, "HOURS", task);
        }

        public void IntervalInDays(int hour, int min, double interval, Action task)
        {
            interval = interval * 24;
            ScheduleTask(hour, min, interval, "DAYS", task);
        }
    }
}
