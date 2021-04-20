#if !UNITY_WEBGL|| UNITY_EDITOR
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityUseful.IEnumeratorUtils;

namespace UnityUseful.AsyncExtensions
{
    public class AsyncCoroutineRunnerGlobal : MonoBehaviour
    {
        const string instance_name = "[AsyncCoroutineRunner]";

        static AsyncCoroutineRunnerGlobal _instance;

        static bool _show_on_scene = false;

        [SerializeField] int m_current_count;
        [Space]
        [SerializeField] List<string> m_current_names = new List<string>();

        List<IEnumerator> m_running_routines = new List<IEnumerator>();

        public int CurrentCount { get => m_current_count; set => m_current_count = value; }
        public static AsyncCoroutineRunnerGlobal Instance
        {
            get
            {
                if (!_instance)
                {
                    var new_holder = new GameObject(instance_name);
                    _instance = new_holder.AddComponent<AsyncCoroutineRunnerGlobal>();
                }

                return _instance;
            }
        }

        void Awake()
        {
            if (!_show_on_scene)
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            Debug.Log("[AsyncCoroutineRunner.<color=red>OnDestroy</color>]");
            this.StopAllCoroutinesLogged(this);
        }
        public static void StartRoutine(IEnumerator routine, string name = "")
        {
            Instance.StartCoroutine(Instance.StartRoutineIE(routine, name));
        }

        IEnumerator StartRoutineIE(IEnumerator routine, string name)
        {
            m_current_count++;
            m_current_names.Add(name);
            m_running_routines.Add(routine);

            yield return routine;

            m_running_routines.Remove(routine);
            m_current_names.Remove(name);
            m_current_count--;
        }
    } 
}
#endif