using GorillaInfoWatch.Tools;
using System;
using System.Linq;

namespace GorillaInfoWatch.Extensions
{
    public static class InvocationExtensions
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
                    Logging.Error(ex);
                }
            }
        }

        public static void SafeInvoke<T1>(this Action<T1> action, T1 arg1)
        {
            object[] parameters = [arg1];

            foreach (Action<T1> invocation in action.GetInvocationList().Cast<Action<T1>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, parameters);
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            object[] parameters = [arg1, arg2];

            foreach (Action<T1, T2> invocation in action.GetInvocationList().Cast<Action<T1, T2>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, parameters);
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }
            }
        }

        public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            object[] parameters = [arg1, arg2, arg3];

            foreach (Action<T1, T2, T3> invocation in action.GetInvocationList().Cast<Action<T1, T2, T3>>())
            {
                try
                {
                    invocation?.Method?.Invoke(invocation?.Target, parameters);
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }
            }
        }
    }
}
