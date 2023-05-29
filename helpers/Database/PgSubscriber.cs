using Npgsql;
using System;
using System.Threading;

namespace helpers.Database
{
    public class PgSubscriber
    {
        private readonly Timer _update_timer;
        private readonly int _delay_ms;
        public PgSubscriber(IDBHelper dbHelper, string tag, TimerCallback timerCallback, int delayMs = 1000)
        {
            _delay_ms = delayMs;
            _update_timer = new Timer(timerCallback);
            dbHelper.Subscribe(tag, ResetCallbackTimer);
        }

        private void ResetCallbackTimer(object sender, NpgsqlNotificationEventArgs e)
        {
            _update_timer.Change(_delay_ms, 0);
            Console.WriteLine(e.Payload);
        }

    }
}
