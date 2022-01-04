using System.Threading.Tasks;

namespace GameFrame.UI
{
    public delegate Task UITransitionEnd();

    public interface IUITransition
    {
        public bool IsActive { get; }

        //return can be null
        UITransitionEnd Transition(string nextUI);
    }
}