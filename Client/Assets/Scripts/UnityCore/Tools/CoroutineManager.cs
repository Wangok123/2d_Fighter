using System.Collections;
using UnityEngine;

namespace UnityCore.Tools
{
    public class CoroutineManager
    {
        private static CoroutineManager _instance;
        public static CoroutineManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CoroutineManager();
                return _instance;
            }
        }

        /// <summary>
        /// 协程启动器
        /// </summary>
        public MonoBehaviour Behaviour { get; set; }
        /// <summary>
        /// 开启一个协程
        /// </summary>
        public void StartCoroutine(IEnumerator enumerator)
        {
            Behaviour.StartCoroutine(enumerator);
        }
        
        public Coroutine Coroutine(IEnumerator enumerator)
        {
            return Behaviour.StartCoroutine(enumerator);
        }
    }
}