using System;
using System.Threading.Tasks;

namespace GameFrame.Flux
{
    public interface IStore : ICloneable
    {
        bool GetDirty();
        void BeforeClearDirty();
        void ClearDirty();
        void SetDirty();
        Task ReceiveAction(IAction uiAction);
        void OnAbort();
        void OnUndoable();
        void OnUndo(IStore prevStore);
        void Init();
        void DeInit();
    }
}