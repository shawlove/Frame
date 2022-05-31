using System.Threading.Tasks;

namespace GameFrame.UI
{
    /// <summary>
    /// return can be null
    /// </summary>
    public delegate Task UITransitionEnd();

    public interface IUITransition
    {
        public bool IsActive { get; }

        //return can be null
        UITransitionEnd Transition(string nextUI);
    }
}