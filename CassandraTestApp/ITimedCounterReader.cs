using System.Threading.Tasks;

namespace CassandraTestApp
{
    public interface ITimedCounterReader
    {
        Task ExecuteAsync();

        void StartStopWatch();
    }
}