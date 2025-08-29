using System;

namespace GorillaInfoWatch.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PreserveScreenSectionAttribute : Attribute
    {
        public bool ClearContent = false;
    }
}