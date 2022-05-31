using System.Threading.Tasks;

namespace GameFrame
{
    public interface IDeInitializeManager
    {
        void DeInitialize();
    }

    public interface ILazyInitializeManager : IDeInitializeManager
    {
        void Initialize();
    }

    public interface ISyncInitalizeManager : IDeInitializeManager
    {
        Task Initialize();
    }
    
}