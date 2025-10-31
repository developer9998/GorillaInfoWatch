using System;

namespace GorillaInfoWatch.Models.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ShowOnHomeScreenAttribute : Attribute
{
    public string DisplayTitle;
}