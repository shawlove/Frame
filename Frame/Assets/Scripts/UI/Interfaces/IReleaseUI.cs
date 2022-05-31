using System.Collections.Generic;

namespace GameFrame.UI
{
    public interface IReleaseUI
    {
        IEnumerable<string> assetAddress { get; }
    }
}