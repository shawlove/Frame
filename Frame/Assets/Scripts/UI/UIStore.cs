using GameFrame.Flux;

namespace GameFrame.UI
{
    public class UIStore : IStore
    {
        public void Receive(IAction action)
        {
            
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}