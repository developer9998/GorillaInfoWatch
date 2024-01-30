using BepInEx.Configuration;
using Bepinject;
using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaInfoWatch.Tools
{
    public class Configuration
    {
        private readonly ConfigFile File;

        public ConfigEntry<int> RefreshRate;

        public Configuration(BepInConfig config)
        {
            File = config.Config;

            RefreshRate = File.Bind("Appearance", "Refresh Rate", 4, "The amount of times the menu refreshes each second");
        }
    }
}
