using System;

namespace GorillaInfoWatch.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ModdedRestrictedScreenAttribute : Attribute;
}