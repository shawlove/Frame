using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrame.Flux;

namespace GameFrame.UI
{
    public class UIManager : MonoSingleton<UIManager>
    {
        public UILoader loader;
        
        private readonly Queue<UIOrder>             _order   = new Queue<UIOrder>();
        private readonly Dictionary<string, UIBase> _uiCache = new Dictionary<string, UIBase>();

        
        private readonly HashSet<IUITransition> _uiTransitions = new HashSet<IUITransition>();
        
        //load form json
        private readonly Dictionary<string, UIConfig> _uiNameInfos = new Dictionary<string, UIConfig>();


        #region Transition

        public void RegisterUITransition(IUITransition transition)
        {
            _uiTransitions.Add(transition);
        }

        public void UnRegisterUITransition(IUITransition transition)
        {
            _uiTransitions.Remove(transition);
        }

        public UITransitionEnd StartUITransition(string nextUI)
        {
            List<UITransitionEnd> ends = new List<UITransitionEnd>();

            //call all active UITransition
            foreach (IUITransition uiTransition in _uiTransitions)
            {
                if (uiTransition.IsActive)
                {
                    var end = uiTransition.Transition(nextUI);
                    if (end != null)
                        ends.Add(uiTransition.Transition(nextUI));
                }
            }

            return EndUITransition;

            async Task EndUITransition()
            {
                foreach (UITransitionEnd end in ends)
                {
                    await end();
                }
            }
        }

        #endregion


        #region Order

        private struct UIOrder
        {
            public string uiName;
            public int    order;
        }

        #endregion


        public async Task OpenUIMiddleware(IAction action)
        {
            if (action.ActionType != ActionType.OPEN_UI) return;

            //if action is open ui , load prefab
            string nextUI = action.GetData1<string>();

            //start transition
            var end = StartUITransition(nextUI);

            //on load done
            await end();

            //hide cur UI
            EuiType type = _uiNameInfos[nextUI].uiType;
            if (type == EuiType.Panel)
            {
                //hide prev panel and popUps
            }

            //record order
            _order.Enqueue(new UIOrder() {uiName = nextUI, order = _order.Count + 1});
        }
    }
}