using BepInEx;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaInfoWatch
{
    public static class RassModLib
    {
        public class RassMod
        {
            public Type FrameworkType;
            public object FrameworkObject;

            public string Name => (string)AccessTools.Property(FrameworkType, "Name").GetValue(FrameworkObject);
            public string Description => (string)AccessTools.Property(FrameworkType, "Description").GetValue(FrameworkObject);
            public bool Active
            {
                get => (bool)AccessTools.Property(FrameworkType, "Enabled").GetValue(FrameworkObject);
                set => AccessTools.Property(FrameworkType, "Enabled").SetValue(FrameworkObject, value);
            }

            public void Update() => AccessTools.Method(FrameworkType, "Update").Invoke(FrameworkObject, null);
            public void OnEnabled() => AccessTools.Method(FrameworkType, "OnEnabled").Invoke(FrameworkObject, null);
            public void OnDisabled() => AccessTools.Method(FrameworkType, "OnDisabled").Invoke(FrameworkObject, null);

            public override string ToString() => string.Format("{0} / {1}: {2}", FrameworkObject.GetType().Name, Name, Description);
        }

        private static List<PluginInfo> _pluginInfos = [];
        private static BaseUnityPlugin _rassMobilePlugin;

        private static readonly List<RassMod> _mods = [];

        public static void InitializeLib(Dictionary<string, PluginInfo> loadedPlugins)
        {
            _pluginInfos = [.. loadedPlugins.Values];
            PluginInfo rassModPluginInfo = _pluginInfos.FirstOrDefault(plugin => plugin.Metadata.GUID == "RassMobile");

            if (rassModPluginInfo != null)
            {
                _rassMobilePlugin = rassModPluginInfo.Instance;

                Assembly rassModAssembly = _rassMobilePlugin.GetType().Assembly;
                Type rassModFWType = rassModAssembly.GetType("RassMobile.ModFramework");
                IList rassModList = (IList)AccessTools.Field(_rassMobilePlugin.GetType(), "mods").GetValue(_rassMobilePlugin);

                foreach (object rassModObject in rassModList)
                {
                    RassMod mod = new()
                    {
                        FrameworkType = rassModFWType,
                        FrameworkObject = rassModObject
                    };

                    Logging.Info(mod.ToString());
                    _mods.Add(mod);
                }
            }
        }
    }
}
