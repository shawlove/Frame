using UnityEngine;

namespace GameFrame.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ContainerUI<T> : UIBase, ICanvasGroupUI
    {
        [HideInInspector]
        [SerializeField]
        private CanvasGroup _canvasGroup;

        public CanvasGroup canvasGroup
        {
            get
            {
                if (ReferenceEquals(_canvasGroup, null))
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }
    }
}