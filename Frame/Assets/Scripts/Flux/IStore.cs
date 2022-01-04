namespace GameFrame.Flux
{
    public interface IStore
    {
        void Receive(IAction action);
    }
}