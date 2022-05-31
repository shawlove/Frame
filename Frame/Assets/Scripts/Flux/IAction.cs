using GameFrame.GenericData;

namespace GameFrame.Flux
{
    public interface IAction : IGenericData
    {
        string ActionType { get; }
    }
}