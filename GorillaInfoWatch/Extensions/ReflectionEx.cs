using HarmonyLib;
using System.Reflection;

namespace GorillaInfoWatch.Extensions
{
    // https://github.com/ToniMacaroni/ComputerInterface/blob/50468f20b4bb7e755d933f8c63627d8cf9394a0e/ComputerInterface/ReflectionEx.cs
    public static class ReflectionEx
    {
        public static void InvokeMethod(this object obj, string name, params object[] par)
        {
            MethodInfo method = AccessTools.Method(obj.GetType(), name);
            method.Invoke(obj, par);
        }

        public static void SetField(this object obj, string name, object value)
        {
            FieldInfo field = AccessTools.Field(obj.GetType(), name);
            field.SetValue(obj, value);
        }

        public static T GetField<T>(this object obj, string name)
        {
            FieldInfo field = AccessTools.Field(obj.GetType(), name);
            return (T)field.GetValue(obj);
        }
    }
}
