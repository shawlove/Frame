using GameFrame.Flux;

namespace GameFrame.Flux
{
    public interface IStoreFactory
    {
        IStore CreateStore(string storeName);
    }
}