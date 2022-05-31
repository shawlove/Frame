using System.Threading.Tasks;

namespace GameFrame
{
    public interface IDeInitializeSingleton
    {
        void DeInitialize();
    }

    public interface ILazyInitializeSingleton : IDeInitializeSingleton
    {
        void Initialize();
    }

    public interface ISyncInitalizeSingleton : IDeInitializeSingleton
    {
        Task Initialize();
    }
    
}