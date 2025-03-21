using System;

namespace GorillaInfoWatch.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayInHomePage(string displayName) : Attribute
    {
        public string DisplayName = displayName;
    }
}
