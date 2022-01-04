namespace GameFrame
{
    public interface IUpdateManager
    {
        /// <summary>
        /// 从小到大
        /// </summary>
        EUpdateManagerOrder UpdateOrder { get; }
        void CustomUpdate();
    }

    //方便查看更改顺序
    public enum EUpdateManagerOrder
    {
        
    }
}