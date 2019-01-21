using System;
using System.Threading.Tasks;

namespace DatabaseFiller
{
    public class TimedHostedService : ITimedHostedService
    {
        private readonly ICounter _counter;

        public TimedHostedService(ICounter counter)
        {
            _counter = counter;
        }

        public async Task ExecuteAsync()
        {
            while (true)
            {
                Console.WriteLine($"Items in queue: {_counter.ReadCounter()}");

                await Task.Delay(1000);
            }
        }
    }
}