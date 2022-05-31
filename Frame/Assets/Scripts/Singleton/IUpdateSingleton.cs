namespace GameFrame
{
    public interface IUpdateSingleton
    {
        /// <summary>
        ///  A smaller number indicates a higher priority
        /// </summary>
        EUpdateSingletonOrder updateOrder { get; }
        void CustomUpdate();
    }

    public enum EUpdateSingletonOrder
    {
        StoreCenter
    }
}