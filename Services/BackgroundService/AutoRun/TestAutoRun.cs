using helpers.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace BackgroundService.AutoRun
{
    public class TestAutoRun : IAutoRun
    {
        public TestAutoRun(IConfiguration config)
        {
        }
        public async Task Start()
        {
            Console.WriteLine($"{nameof(TestAutoRun)} has started automatically and will be executed once");
        }
    }
}
