using System;

namespace helpers.Interfaces
{
    public interface ISchedulerProvider
    {
        void IntervalInDays(int hour, int min, double interval, Action task);
        void IntervalInHours(int hour, int min, double interval, Action task);
        void IntervalInMinutes(int hour, int min, double interval, Action task);
        void IntervalInSeconds(int hour, int sec, double interval, Action task);
    }
}