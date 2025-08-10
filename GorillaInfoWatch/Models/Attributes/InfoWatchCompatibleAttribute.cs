using System;

namespace GorillaInfoWatch.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class InfoWatchCompatibleAttribute : Attribute;
}
