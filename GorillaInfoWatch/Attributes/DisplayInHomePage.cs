using System;

namespace GorillaInfoWatch.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayInHomePage(string displayName, int displayOrder = 0, bool isDebug = false) : Attribute
    {
        public string DisplayName = displayName;
        public int DisplayOrder = displayOrder;
        public bool IsDebug = isDebug;
    }
}
