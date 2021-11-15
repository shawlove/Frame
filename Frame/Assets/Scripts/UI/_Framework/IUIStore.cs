namespace GameFrame.UI
{
    public interface IUIStore
    {
        void Receive(IUIAction action);
    }
}