using System;

namespace GameFrame.Flux
{
    public interface IStore : ICloneable
    {
        void Receive(IAction action);
    }
}