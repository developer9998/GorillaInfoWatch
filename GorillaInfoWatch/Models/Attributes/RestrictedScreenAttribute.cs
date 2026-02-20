using System;

namespace GorillaInfoWatch.Models.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class RestrictedScreenAttribute : Attribute;