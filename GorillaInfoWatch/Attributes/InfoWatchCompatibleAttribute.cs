using System;

namespace GorillaInfoWatch.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class InfoWatchCompatibleAttribute : Attribute;
}
