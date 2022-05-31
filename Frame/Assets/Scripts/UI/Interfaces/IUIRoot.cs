using System.Collections.Generic;

namespace GameFrame.UI
{
    /// <summary>
    /// panel popUp etc.
    /// </summary>
    public interface IUIRoot
    {
        IEnumerable<UIBase> allUI { get; }
    }
}