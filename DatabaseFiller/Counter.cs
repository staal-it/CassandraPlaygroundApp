using System.Threading;

namespace DatabaseFiller
{
    public class Counter : ICounter
    {
        private long _sharedInteger;

        public void IncrementCounter()
        {
            Interlocked.Increment(ref _sharedInteger);
        }

        public long ReadCounter()
        {
            return Interlocked.Read(ref _sharedInteger);
        }
    }
}