using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Extensions;

internal static class ConfigExtensions
{
    public static IEnumerable<ConfigEntryBase> GetEntries(this ConfigFile file) => file.Keys.Select(key => file[key]);
}
