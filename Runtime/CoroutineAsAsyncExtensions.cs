#if !UNITY_WEBGL //Not support for System.Threading in WEBGL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;


namespace UnityUseful.AsyncExtensions
{
    public static class CoroutineAsAsyncExtensions
    {
        public static TaskAwaiter<int> GetAwaiter(this Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;

            process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);

            if (process.HasExited)
            {
                tcs.TrySetResult(process.ExitCode);
            }

            return tcs.Task.GetAwaiter();
        }
        public static async Task<T> WrapErrors<T>(this Task<T> task)
        {
            return await task;
        }
        public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan)
        {
            return Task.Delay(timeSpan).GetAwaiter();
        }
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }

        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }
        }
        public static IEnumerator<T> AsIEnumerator<T>(this Task<T> task)
        {
            while (!task.IsCompleted)
            {
                yield return default(T);
            }

            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }

            yield return task.Result;
        }
    }

    public class UnityWebRequestAwaiter : INotifyCompletion
    {
        private UnityWebRequestAsyncOperation asyncOp;
        private Action continuation;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
            asyncOp.completed += OnRequestCompleted;
        }

        public bool IsCompleted { get { return asyncOp.isDone; } }

        public void GetResult() { }

        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }

        void OnRequestCompleted(AsyncOperation obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Debug.Log("[AsyncExtension <color=red>Important!</color>] Callback trying to finish not in \"Play\" Mode! Ignore continuation...");
                return;
            }
#endif
            continuation();
        }
    }
    public class WaitForBackgroundThread
    {
        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            return Task.Run(() => { }).ConfigureAwait(false).GetAwaiter();
        }
    }
}

#endif