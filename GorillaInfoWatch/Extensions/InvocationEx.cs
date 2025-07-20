using GorillaInfoWatch.Tools;
using System;
using System.Linq;

namespace GorillaInfoWatch.Extensions
{
    public static class InvocationEx
    {
        public static void SafeInvoke(this Action action)
        {
            foreach (Action invocation in action.GetInvocationList().Cast<Action>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, null);
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Error invocating method");
                    Logging.Error(ex);
                }
            }
        }

        public static void SafeInvoke<T1>(this Action<T1> action, T1 arg1)
        {
            foreach (Action<T1> invocation in action.GetInvocationList().Cast<Action<T1>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, [arg1]);
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Error invocating method with one parameter");
                    Logging.Error(ex);
                }
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            foreach (Action<T1, T2> invocation in action.GetInvocationList().Cast<Action<T1, T2>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, [arg1, arg2]);
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Error invocating method with two parameters");
                    Logging.Error(ex);
                }
            }
        }

        public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            foreach (Action<T1, T2, T3> invocation in action.GetInvocationList().Cast<Action<T1, T2, T3>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, [arg1, arg2, arg3]);
                }
                catch (Exception ex)
                {
                    Logging.Fatal("Error invocating method with three parameters");
                    Logging.Error(ex);
                }
            }
        }
    }
}
