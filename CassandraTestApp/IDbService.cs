using System.Threading.Tasks;

namespace CassandraTestApp
{
    public interface IDbService
    {
        Task Start();
    }
}