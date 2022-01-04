using GameFrame.GenericData;

namespace GameFrame.Flux
{
    public interface IAction : IGenericData
    {
        int ActionType { get; }
    }
}