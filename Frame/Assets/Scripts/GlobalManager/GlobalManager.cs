using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// 全局管理类通用单例基类
    /// 由GameDirector统一管理
    /// </summary>
    public abstract class GlobalManager<T> : MonoBehaviour
    {
        public static T Instance { get; private set; }
    }
}