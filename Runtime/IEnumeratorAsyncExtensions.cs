﻿    using UnityEngine;

    using System;
    using System.Linq;
    using System.Threading;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;

namespace UnityUseful.AsyncExtensions
{
    public static class IEnumeratorAwaitExtensions
    {
        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForSeconds instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForUpdate instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForEndOfFrame instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForFixedUpdate instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForSecondsRealtime instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitUntil instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitWhile instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter<AsyncOperation> GetAwaiter(this AsyncOperation instruction)
        {
            return GetAwaiterReturnSelf(instruction);
        }

        public static SimpleCoroutineAwaiter<UnityEngine.Object> GetAwaiter(this ResourceRequest instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<UnityEngine.Object>();
            var resource_request = InstructionWrappers.ResourceRequest(awaiter, instruction);

            RunOnUnityScheduler(() => AsyncCoroutineRunnerGlobal.StartRoutine(resource_request, nameof(GetAwaiter) + "(UnityObject)"));

            return awaiter;
        }

        public static SimpleCoroutineAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<AssetBundle>();
            var asset_create_bundle_request = InstructionWrappers.AssetBundleCreateRequest(awaiter, instruction);

            RunOnUnityScheduler(() => AsyncCoroutineRunnerGlobal.StartRoutine(asset_create_bundle_request, nameof(AssetBundleCreateRequest)));

            return awaiter;
        }

        public static SimpleCoroutineAwaiter<UnityEngine.Object> GetAwaiter(this AssetBundleRequest instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<UnityEngine.Object>();
            var asset_bundle_request = InstructionWrappers.AssetBundleRequest(awaiter, instruction);

            RunOnUnityScheduler(() => AsyncCoroutineRunnerGlobal.StartRoutine(asset_bundle_request, nameof(AssetBundleRequest)));

            return awaiter;
        }

        public static SimpleCoroutineAwaiter<T> GetAwaiter<T>(this IEnumerator<T> coroutine)
        {
            var awaiter = new SimpleCoroutineAwaiter<T>();
            var simple_routine = new CoroutineWrapper<T>(coroutine, awaiter).Run();

            RunOnUnityScheduler(() => AsyncCoroutineRunnerGlobal.StartRoutine(simple_routine, nameof(GetAwaiter) + "<" + typeof(T).Name + ">"));

            return awaiter;
        }

        public static SimpleCoroutineAwaiter<object> GetAwaiter(this IEnumerator coroutine)
        {
            var awaiter = new SimpleCoroutineAwaiter<object>();
            var simple_routine = new CoroutineWrapper<object>(coroutine, awaiter).Run();

            RunOnUnityScheduler(() => AsyncCoroutineRunnerGlobal.StartRoutine(simple_routine, nameof(GetAwaiter) + "(object)"));

            return awaiter;
        }

        static SimpleCoroutineAwaiter GetAwaiterReturnVoid(object instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter();
            var return_void = InstructionWrappers.ReturnVoid(awaiter, instruction);

            RunOnUnityScheduler(() => AsyncCoroutineRunnerGlobal.StartRoutine(return_void));

            return awaiter;
        }

        static SimpleCoroutineAwaiter<T> GetAwaiterReturnSelf<T>(T instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<T>();
            var return_self = InstructionWrappers.ReturnSelf(awaiter, instruction);

            RunOnUnityScheduler(() => AsyncCoroutineRunnerGlobal.StartRoutine(return_self));

            return awaiter;
        }

        static void RunOnUnityScheduler(Action action)
        {
            if (SynchronizationContext.Current == SyncContextUtil.UnitySynchronizationContext)
            {
                action();
            }
            else
            {
                SyncContextUtil.UnitySynchronizationContext.Post(_ => action(), null);
            }
        }

        static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Assert hit in UnityAsyncUtil package!");
            }
        }

        public class SimpleCoroutineAwaiter<T> : INotifyCompletion
        {
            bool _isDone;
            Exception _exception;
            Action _continuation;
            T _result;

            public bool IsCompleted
            {
                get { return _isDone; }
            }

            public T GetResult()
            {
                Assert(_isDone);

                if (_exception != null)
                {
                    ExceptionDispatchInfo.Capture(_exception).Throw();
                }

                return _result;
            }

            public void Complete(T result, Exception e)
            {
                Assert(!_isDone);

                _isDone = true;
                _exception = e;
                _result = result;

                // Always trigger the continuation on the unity thread when awaiting on unity yield
                // instructions
                if (_continuation != null)
                {
                    RunOnUnityScheduler(_continuation);
                }
            }

            void INotifyCompletion.OnCompleted(Action continuation)
            {
                Assert(_continuation == null);
                Assert(!_isDone);

                _continuation = continuation;
            }
        }

        public class SimpleCoroutineAwaiter : INotifyCompletion
        {
            bool _isDone;
            Exception _exception;
            Action _continuation;

            public bool IsCompleted
            {
                get { return _isDone; }
            }

            public void GetResult()
            {
                Assert(_isDone);

                if (_exception != null)
                {
                    ExceptionDispatchInfo.Capture(_exception).Throw();
                }
            }

            public void Complete(Exception e)
            {
                Assert(!_isDone);

                _isDone = true;
                _exception = e;

                // Always trigger the continuation on the unity thread when awaiting on unity yield
                // instructions
                if (_continuation != null)
                {
                    RunOnUnityScheduler(_continuation);
                }
            }

            void INotifyCompletion.OnCompleted(Action continuation)
            {
                Assert(_continuation == null);
                Assert(!_isDone);

                _continuation = continuation;
            }
        }

        class CoroutineWrapper<T>
        {
            readonly SimpleCoroutineAwaiter<T> _awaiter;
            readonly Stack<IEnumerator> _processStack;

            public CoroutineWrapper(IEnumerator coroutine, SimpleCoroutineAwaiter<T> awaiter)
            {
                _processStack = new Stack<IEnumerator>();
                _processStack.Push(coroutine);
                _awaiter = awaiter;
            }

            public IEnumerator Run()
            {
                while (true)
                {
                    var topWorker = _processStack.Peek();

                    bool isDone;

                    try
                    {
                        isDone = !topWorker.MoveNext();
                    }
                    catch (Exception e)
                    {
                        // The IEnumerators we have in the process stack do not tell us the
                        // actual names of the coroutine methods but it does tell us the objects
                        // that the IEnumerators are associated with, so we can at least try
                        // adding that to the exception output
                        var objectTrace = GenerateObjectTrace(_processStack);

                        if (objectTrace.Any())
                        {
                            _awaiter.Complete(default, new Exception(GenerateObjectTraceMessage(objectTrace), e));
                        }
                        else
                        {
                            _awaiter.Complete(default, e);
                        }

                        yield break;
                    }

                    if (isDone)
                    {
                        _processStack.Pop();

                        if (_processStack.Count == 0)
                        {
                            _awaiter.Complete((T)topWorker.Current, null);
                            yield break;
                        }
                    }

                    // We could just yield return nested IEnumerator's here but we choose to do
                    // our own handling here so that we can catch exceptions in nested coroutines
                    // instead of just top level coroutine
                    if (topWorker.Current is IEnumerator)
                    {
                        _processStack.Push((IEnumerator)topWorker.Current);
                    }
                    else
                    {
                        // Return the current value to the unity engine so it can handle things like
                        // WaitForSeconds, WaitToEndOfFrame, etc.
                        yield return topWorker.Current;
                    }
                }
            }

            string GenerateObjectTraceMessage(List<Type> objTrace)
            {
                var result = new System.Text.StringBuilder();

                foreach (var objType in objTrace)
                {
                    if (result.Length != 0)
                    {
                        result.Append(" -> ");
                    }

                    result.Append(objType.ToString());
                }

                result.AppendLine();
                return "Unity Coroutine Object Trace: " + result.ToString();
            }

            static List<Type> GenerateObjectTrace(IEnumerable<IEnumerator> enumerators)
            {
                var objTrace = new List<Type>();

                foreach (var enumerator in enumerators)
                {
                    // NOTE: This only works with scripting engine 4.6
                    // And could easily stop working with unity updates
                    var field = enumerator.GetType().GetField("$this", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                    if (field == null)
                    {
                        continue;
                    }

                    var obj = field.GetValue(enumerator);

                    if (obj == null)
                    {
                        continue;
                    }

                    var objType = obj.GetType();

                    if (!objTrace.Any() || objType != objTrace.Last())
                    {
                        objTrace.Add(objType);
                    }
                }

                objTrace.Reverse();
                return objTrace;
            }
        }


    } 
}