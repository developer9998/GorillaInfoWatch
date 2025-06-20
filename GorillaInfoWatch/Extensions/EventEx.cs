using System;
using System.Linq;

namespace GorillaInfoWatch.Extensions
{
    public static class EventEx
    {
        public static void SafeInvoke(this Action action)
        {
            foreach (var invocation in (action?.GetInvocationList()).Cast<Action>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, null);
                }
                catch
                {

                }
            }
        }

        public static void SafeInvoke<T1>(this Action<T1> action, T1 arg1)
        {
            foreach (var invocation in (action?.GetInvocationList()).Cast<Action<T1>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, [arg1]);
                }
                catch
                {

                }
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            foreach (var invocation in (action?.GetInvocationList()).Cast<Action<T1, T2>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, [arg1, arg2]);
                }
                catch
                {

                }
            }
        }
    }
}
