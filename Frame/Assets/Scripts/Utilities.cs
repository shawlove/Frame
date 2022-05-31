using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace GameFrame
{
    public static class Utilities
    {
        #region 快速字符串哈希 来源：http: //isthe.com/chongo/src/fnv/hash_64a.c

        static readonly ulong   INIT_HASH_VALUE = 0xcbf29ce484222325UL;
        static readonly ulong   PRIME_LOW       = 0x1b3;
        static readonly int     PRIME_SHIFT     = 8;
        private static  ulong[] _cacheVal       = new ulong[4];
        private static  ulong[] _cacheTemp      = new ulong[4];

        /// <summary>
        /// 快速字符串哈希
        /// </summary>
        public static ulong StringHashFnv1a(this string s)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(s);
            uint   low      = (uint) INIT_HASH_VALUE;
            uint   high     = (uint) (INIT_HASH_VALUE >> 32);
            _cacheVal[0] =  low;
            _cacheVal[1] =  low >> 16;
            _cacheVal[0] &= 0xffff;
            _cacheVal[2] =  high;
            _cacheVal[3] =  (_cacheVal[2] >> 16);
            _cacheVal[2] &= 0xffff;

            for (int i = 0; i < strBytes.Length; i++)
            {
                _cacheTemp[0] = _cacheVal[0] * PRIME_LOW;
                _cacheTemp[1] = _cacheVal[1] * PRIME_LOW;
                _cacheTemp[2] = _cacheVal[2] * PRIME_LOW;
                _cacheTemp[3] = _cacheVal[3] * PRIME_LOW;

                _cacheTemp[2] += _cacheVal[0] << PRIME_SHIFT;
                _cacheTemp[3] += _cacheVal[1] << PRIME_SHIFT;

                _cacheTemp[1] += (_cacheTemp[0] >> 16);
                _cacheVal[0]  =  _cacheTemp[0] & 0xffff;
                _cacheTemp[2] += (_cacheTemp[1] >> 16);
                _cacheVal[1]  =  _cacheTemp[1] & 0xffff;
                _cacheVal[3]  =  _cacheTemp[3] + (_cacheTemp[2] >> 16);
                _cacheVal[2]  =  _cacheTemp[2] & 0xffff;
                _cacheVal[0]  ^= (ulong) strBytes[i];
            }

            high = (uint) ((_cacheVal[3] << 16) | _cacheVal[2]);
            low  = (uint) ((_cacheVal[1] << 16) | _cacheVal[0]);
            ulong res = ((ulong) high) << 32;
            res += low;
            return res;
        }

        #endregion


        public static T GetScriptBehaviour<T>(this Playable playable)
            where T : class, IPlayableBehaviour, new()
        {
            ScriptPlayable<T> scriptPlayable = (ScriptPlayable<T>) playable;
            return scriptPlayable.GetBehaviour();
        }

        public static void Switch(this CanvasGroup canvasGroup, bool isShow)
        {
            canvasGroup.alpha          = isShow ? 1 : 0;
            canvasGroup.interactable   = isShow;
            canvasGroup.blocksRaycasts = isShow;
        }

        public static async void Await(this Task task)
        {
            await task;
        }
    }
}