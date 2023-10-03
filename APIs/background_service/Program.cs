using helpers.Engine.Scheduler;
using Serilog;
using System;
using System.Runtime.Loader;
using System.Threading;

namespace queue_background_services
{
    public class Program
    {
        static string _backgroundServiceName = "Background Service";
        static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += OnUnloadingEventHandler;
            Console.CancelKeyPress += CancelHandler;

            using (var cts = new CancellationTokenSource())
            {
                var startup = new Startup();
                startup.GetService<TaskScheduleEngine>();

                var waitHandles = new WaitHandle[] { cts.Token.WaitHandle };
                int signaledIndex = WaitHandle.WaitAny(waitHandles);

                if (signaledIndex == 0)
                    Console.WriteLine("Cancellation requested.");
            }

            Log.Information($"{_backgroundServiceName} reached tail");
        }

        private static void OnUnloadingEventHandler(AssemblyLoadContext obj)
        {
            Log.Information($"{_backgroundServiceName} was stopped on OnUnloadingEventHandler");
        }
        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            Log.Information($"{_backgroundServiceName} was cancelled on CancelHandler");
        }
    }
}
