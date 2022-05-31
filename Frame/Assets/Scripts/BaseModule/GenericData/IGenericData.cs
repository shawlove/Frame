using GameFrame.Recycle;

namespace GameFrame.GenericData
{
    public interface IGenericData : IRecycle
    {
        T GetData1<T>();
        T GetData2<T>();
        T GetData3<T>();
        T GetData4<T>();
    }
}