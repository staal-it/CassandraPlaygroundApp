using System.Threading;
using System.Threading.Tasks;

namespace DatabaseFiller
{
    public interface ITimedHostedService
    {
        Task ExecuteAsync();
    }
}