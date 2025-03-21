using System;

namespace GorillaInfoWatch.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class WatchCustomPageAttribute : Attribute;
}