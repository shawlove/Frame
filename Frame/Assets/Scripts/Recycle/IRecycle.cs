namespace GameFrame.Recycle
{
    public interface IRecycle
    {
        /// <summary>
        /// get set by RecycleList
        /// </summary>
        int Index { get; set; }

        void Reset();

        void Recycle();
    }
}