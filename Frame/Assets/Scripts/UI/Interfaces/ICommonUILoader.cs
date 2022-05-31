using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameFrame.UI
{
    public interface ICommonUILoader
    {
        IEnumerable<Task> LoadCommonUI();
    }
}