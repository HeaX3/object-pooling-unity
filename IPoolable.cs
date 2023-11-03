namespace ObjectPooling
{
    public interface IPoolable
    {
        void Activate();
        void ResetForPool();
    }
}