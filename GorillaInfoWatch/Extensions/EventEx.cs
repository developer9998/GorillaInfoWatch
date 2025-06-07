using System;
using System.Linq;

namespace GorillaInfoWatch.Extensions
{
    public static class EventEx
    {
        public static void SafeInvoke<T1>(this Action<T1> action, params object[] args)
        {
            foreach (var invocation in (action?.GetInvocationList()).Cast<Action<T1>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, args);
                }
                catch
                {

                }
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, params object[] args)
        {
            foreach (var invocation in (action?.GetInvocationList()).Cast<Action<T1, T2>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, args);
                }
                catch
                {

                }
            }
        }
    }
}
