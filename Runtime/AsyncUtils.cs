#if !UNITY_WEBGL
//#define ASYNC_DEBUG;

using System;
using System.Threading.Tasks;

using UnityEngine;

namespace UnityUseful.AsyncExtensions
{

    public static class AsyncUtils
    {
        const string m_async_block = "<b>AsyncExtensions.EditorCheckPlayMode</b> Preventing async code to execute after exit from Unity Play Mode!";
        public static async Task ReverseTimer(int seconds, Action<int> OnSecondTick, Func<bool> break_condition, params Action[] finals)
        {
            while (seconds > 0 && !break_condition.Invoke())
            {
                EditorCheckPlayMode();

                seconds--;
                OnSecondTick(seconds);

                await Delay(1f);
            }
            FinilizeActions(finals);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="OnFrameTime">x->seconds, y-> Time.deltaTime </param>
        /// <param name="break_condition"></param>
        /// <param name="finals"></param>
        /// <returns></returns>
        public static async Task ReverseTimer(float seconds, Action<float, float> OnFrameTime, Func<bool> break_condition, params Action[] finals)
        {
            while (seconds > 0 && !break_condition.Invoke())
            {
                seconds -= Time.deltaTime;

                EditorCheckPlayMode();

                OnFrameTime(seconds, Time.deltaTime);

                await Task.Yield();
            }
            FinilizeActions(finals);
        }
        /// <summary>
        /// Timer with progress callbacks
        /// </summary>
        /// <param name="time"></param>
        /// <param name="prog_and_delta">x->progress, y->deltaTime</param>
        /// <param name="finals"></param>
        /// <returns></returns>
        public static async Task ProgressTimer(float time, Action<float, float> prog_and_delta, bool unscaled = false, params Action[] finals)
        {
            var cur_time = 0f;

            while (time > cur_time)
            {
                var delta = unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                cur_time += delta;

                EditorCheckPlayMode();

                var progress = Mathf.Clamp01(cur_time / time);
                prog_and_delta(progress, delta);

                await Task.Yield();

            }
            FinilizeActions(finals);
        }

        /// <summary>
        /// Throws exception to break continuation of async code when exit from Play Mode
        /// </summary>   
        /// <exception cref="System.NotSupportedException"></exception>
        public static void EditorCheckPlayMode()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                throw new NotSupportedException(m_async_block);
            }
#endif
        }
        /// <summary>
        /// Copy of <see cref="System.Threading.Tasks.Task.Delay(int)"></see> with hadling exit from Unity Play Mode
        /// </summary>
        /// <param name="time">Seconds to wait</param>
        /// <returns></returns>
        public static async Task Delay(float time, bool use_scale = true)
        {
            var int_time = Mathf.FloorToInt(time * 1000);
#if UNITY_EDITOR && ASYNC_DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Debug.Log($"Start, wait: {int_time}, TimeScale: {Time.timeScale}");
#endif

            int_time = use_scale ? (int)(int_time / Time.timeScale) : int_time;
            await Task.Delay(int_time);

            EditorCheckPlayMode();

#if UNITY_EDITOR && ASYNC_DEBUG
            sw.Stop();
            Debug.Log($"End, Elapsed: s:{sw.Elapsed.TotalSeconds}, ms: {sw.ElapsedMilliseconds}");
#endif
        }
        public static async Task WaitWhile(Func<bool> func)
        {
            while (func())
            {
                EditorCheckPlayMode();

                await Task.Yield();
            }
        }
        static void FinilizeActions(Action[] finals)
        {
            if (finals != null)
            {
                foreach (var action in finals)
                {
                    EditorCheckPlayMode();
                    action();
                }
            }
        }
    }

}


#endif