namespace CassandraTestApp
{
    public interface ICounter
    {
        void IncrementCounter()
            ;
        long ReadCounter();
    }
}