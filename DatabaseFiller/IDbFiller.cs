using System.Threading.Tasks;

namespace DatabaseFiller
{
    public interface IDbFiller
    {
        Task Start();
    }
}