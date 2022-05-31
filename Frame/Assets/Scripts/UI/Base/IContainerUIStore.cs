using GameFrame.Flux;

namespace UI.Base
{
    public interface IContainerUIStore : IStore
    {
        public string uiName { get; }

        public bool isShow { get; }

        public bool onShow { get; }
        public bool onHide { get; }

        public bool isOnOpen { get; }
    }
}