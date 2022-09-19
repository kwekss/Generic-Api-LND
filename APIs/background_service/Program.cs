using helpers.Engine;
using helpers.Notifications;
using Microsoft.Extensions.Configuration;
using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace queue_background_services
{
    public class Program
    {
        static string _backgroundServiceName = "Background Service";
        static IMessengerHub _messengerHub;
        static IConfiguration _config;
        static int _delayTime;
        static readonly CancellationTokenSource _cancelationTokensSource = new CancellationTokenSource();
        static async Task Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += OnUnloadingEventHandler;
            Console.CancelKeyPress += CancelHandler;

            var startup = new Startup();
            _config = startup.GetService<IConfiguration>();

            _delayTime = _config.GetValue<int>("BACKGROUND_SERVICE:LOOP_DELAY_SEC", 5);
            int jobInstances = _config.GetValue("BACKGROUND_SERVICE:JOB_INSTANCES", 1);
            _backgroundServiceName = _config.GetValue("BACKGROUND_SERVICE:NAME", "Background Service");

            _messengerHub = startup.GetService<IMessengerHub>();

            _messengerHub.Publish(new LogWriter("info", $"{_backgroundServiceName} was started"));

            startup.GetService<BackgroundRunner>();

            while (!_cancelationTokensSource.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delayTime), _cancelationTokensSource.Token).ConfigureAwait(false);
            }

            Console.WriteLine($"{_backgroundServiceName} reached tail");
        }

        private static void OnUnloadingEventHandler(AssemblyLoadContext obj)
        {
            Console.WriteLine($"{_backgroundServiceName} was stopped on OnUnloadingEventHandler");
            _messengerHub.Publish(new LogWriter("info", $"{_backgroundServiceName} was stopped on OnUnloadingEventHandler"));
        }
        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine($"{_backgroundServiceName} was cancelled on CancelHandler");
            _messengerHub.Publish(new LogWriter("info", $"{_backgroundServiceName} was cancelled on CancelHandler"));
        }
    }
}
