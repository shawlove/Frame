namespace GameFrame.Config
{
    public interface IExcelGenerateAsset
    { 
        void   CreateAsset();
        
        /// <summary>
        /// 检查Asset合理性
        /// </summary>
        string CheckAsset();
    }
}