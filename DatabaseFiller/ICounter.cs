namespace DatabaseFiller
{
    public interface ICounter
    {
        void IncrementCounter()
            ;
        long ReadCounter();
    }
}