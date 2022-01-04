using System.Threading.Tasks;

namespace GameFrame
{
    public interface IInitializeManager
    {
        /// <summary>
        /// 从小到大
        /// </summary>
        EInitializeManagerOrder InitializeOrder { get; }

        Task Initialize();
        void DeInitialize();
    }

    //方便查看更改顺序
    public enum EInitializeManagerOrder
    {
        
    }
}