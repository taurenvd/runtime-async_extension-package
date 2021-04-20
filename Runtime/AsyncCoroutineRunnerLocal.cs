#if !UNITY_WEBGL
using UnityEngine;
using UnityUseful.IEnumeratorUtils;

namespace UnityUseful.AsyncExtensions
{
    public class AsyncCoroutineRunnerLocal : MonoBehaviour
    {
        const string instance_name = "[" + nameof(AsyncCoroutineRunnerLocal) + "]";

        [SerializeField] int m_current_count;

        static AsyncCoroutineRunnerLocal _instance;

        public static AsyncCoroutineRunnerLocal Instance
        {
            get
            {
                if (_instance == null)
                {
                    var new_holder = new GameObject(instance_name);
                    var component = new_holder.AddComponent<AsyncCoroutineRunnerLocal>();

                    _instance = component;
                }

                return _instance;
            }
        }

        public int CurrentCount { get => m_current_count; set => m_current_count = value; }

        //void Awake()
        //{
        //    // Don't show in scene hierarchy
        //    //gameObject.hideFlags = HideFlags.HideAndDontSave;
        //}

        void OnDestroy()
        {
            Debug.LogError("AsyncCoroutineRunner.OnDestroy");
            this.StopAllCoroutinesLogged();
        }
    } 
}
#endif