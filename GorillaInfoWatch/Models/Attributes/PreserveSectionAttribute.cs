using System;

namespace GorillaInfoWatch.Models.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class PreserveSectionAttribute : Attribute
{
    public bool ClearContent = false;
}