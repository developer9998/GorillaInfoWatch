using System;
using System.Reflection;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetCommand
    {
        public readonly Delegate Command;

        public object Target => Command?.Target;
        public MethodInfo Method => Command?.Method;

        public WidgetCommand(Delegate command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));;
        }

        public WidgetCommand(Action action) : this(action as Delegate)
        {

        }

        public WidgetCommand(Action<bool> action) : this(action as Delegate)
        {

        }

        public WidgetCommand(Action<object[]> action) : this(action as Delegate)
        {

        }

        public static implicit operator WidgetCommand(Action action)
        {
            return new WidgetCommand(action);
        }

        public static implicit operator WidgetCommand(Action<bool> action)
        {
            return new WidgetCommand(action);
        }

        public static implicit operator WidgetCommand(Action<object[]> action)
        {
            return new WidgetCommand(action);
        }

        public static implicit operator WidgetCommand(Delegate del)
        {
            return new WidgetCommand(del);
        }

        public void Invoke(object[] args)
        {
            ParameterInfo[] baseParameters = Command.Method.GetParameters();

            if (baseParameters.Length == 0)
            {
                Command.DynamicInvoke(null);
                return;
            }

            object[] parameters = new object[baseParameters.Length];
            bool[] used = new bool[baseParameters.Length];

            for (int i = 0; i < baseParameters.Length; i++)
            {
                Type targetType = baseParameters[i].ParameterType;

                for (int j = 0; j < args.Length; j++)
                {
                    if (used[j])
                        continue;

                    object candidate = args[j];

                    if (candidate is null && (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null))
                    {
                        parameters[i] = null;
                        used[j] = true;
                        break;
                    }
                    else if (candidate is not null && targetType.IsAssignableFrom(candidate.GetType()))
                    {
                        parameters[i] = candidate;
                        used[j] = true;
                        break;
                    }
                }
            }

            Command.DynamicInvoke(parameters);
        }
    }
}
