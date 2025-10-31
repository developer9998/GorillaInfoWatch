using System;

namespace GorillaInfoWatch.Models.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PreserveScreenSectionAttribute : Attribute
{
    public bool ClearContent = false;
}