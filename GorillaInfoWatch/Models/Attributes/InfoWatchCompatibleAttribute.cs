using System;

namespace GorillaInfoWatch.Models.Attributes;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public class InfoWatchCompatibleAttribute : Attribute
{
    public Version MinimumVersion { get; protected set; }

    public InfoWatchCompatibleAttribute()
    {
        MinimumVersion = null;
    }

    public InfoWatchCompatibleAttribute(Version minimumVersion)
    {
        MinimumVersion = minimumVersion;
    }
}
