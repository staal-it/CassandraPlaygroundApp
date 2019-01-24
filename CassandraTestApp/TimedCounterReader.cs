using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CassandraTestApp
{
    public class TimedCounterReader : ITimedCounterReader
    {
        private readonly ICounter _counter;
        private readonly Stopwatch _sw = new Stopwatch();

        public TimedCounterReader(ICounter counter)
        {
            _counter = counter;
        }

        public async Task ExecuteAsync()
        {
            while (true)
            {
                await Task.Delay(1000);

                var items = _counter.ReadCounter();

                long itemsPerSec = 0;
                if (_sw.IsRunning && _sw.Elapsed.Seconds > 0)
                    itemsPerSec = (long) (items / _sw.Elapsed.TotalSeconds);

                Console.WriteLine($"Items in queue: {_counter.ReadCounter()}, per sec: {itemsPerSec}");
            }
        }

        public void StartStopWatch()
        {
            _sw.Start();
        }
    }
}