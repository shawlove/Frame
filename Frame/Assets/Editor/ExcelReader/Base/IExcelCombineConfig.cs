namespace GameFrame.Config
{
    /// <summary>
    /// 可以合并的配置表，如DataConfig的纵表和横表
    /// </summary>
    public interface IExcelCombineConfig
    {
        void Combine(IExcelCombineConfig another);
    }
}