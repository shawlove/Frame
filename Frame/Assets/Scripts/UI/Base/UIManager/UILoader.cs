using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame.UI
{
    public class UILoader
    {
        public async Task<UIBase> GetUI(string path, Transform parent)
        {
            //todo load
            UIBase asset = null;

            if (asset == null)
            {
                Debug.LogError($"{path} load fail");
                return null;
            }

            UIBase uiBase = Object.Instantiate(asset, parent);

            if (asset is IUIRoot uiRoot)
            {
                foreach (UIBase ui in uiRoot.allUI)
                {
                    try
                    {
                        ui.OnInitialize();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            else
            {
                Debug.LogError($"{path} not inherit form IUIRoot");
            }

            if (uiBase is ICanvasGroupUI canvasGroupUI)
            {
                canvasGroupUI.canvasGroup.Switch(false);
            }

            if (uiBase is ICanvasUI canvasUI)
            {
                canvasUI.canvas.sortingOrder = 0;
            }

            if (uiBase is ICommonUILoader commonUILoader)
            {
                foreach (var task in commonUILoader.LoadCommonUI())
                {
                    await task;
                }
            }

            return uiBase;
        }

        public void DisposeUI(UIBase uiBase)
        {
            if (uiBase is IUIRoot uiRoot)
            {
                foreach (UIBase ui in uiRoot.allUI)
                {
                    DeInitialize(ui);
                }
            }
            else
            {
                List<UIBase> uis = new List<UIBase>();

                uiBase.GetComponentsInChildren(true, uis);
                foreach (UIBase ui in uis)
                {
                    DeInitialize(ui);
                }
            }

            void DeInitialize(UIBase ui)
            {
                ui.OnDeInitialize();
                if (ui is IReleaseUI releaseUI)
                {
                    foreach (string address in releaseUI.assetAddress)
                    {
                        //todo release
                    }
                }
            }
        }
    }
}