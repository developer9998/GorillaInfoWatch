using BepInEx.Configuration;
using Bepinject;
using GorillaInfoWatch.Models;
using HarmonyLib;
using System.Reflection;

namespace GorillaInfoWatch.Tools
{
    public class Configuration
    {
        private readonly ConfigFile File;

        public ConfigEntry<int> RefreshRate;

        public ConfigEntry<PresetColourTypes> MenuColour, FavouriteColour;

        public Configuration(BepInConfig config)
        {
            File = config.Config;

            RefreshRate = File.Bind("Appearance", "Refresh Rate", 4, "The amount of times the menu is refreshed each second");

            MenuColour = File.Bind("Appearance", "Menu Colour", PresetColourTypes.Black, "The colour used to serve as the default background colour of the menu");
            FavouriteColour = File.Bind("Appearance", "Favourite Colour", PresetColourTypes.Yellow, "The colour used to serve as a unique identifier for those who you have favourited");
        }

        public void Sync(ConfigEntryBase seperatedEntryBase)
        {
            FieldInfo[] fieldInfo = GetType().GetFields();
            foreach (FieldInfo fI in fieldInfo)
            {
                if (fI.GetValue(this) is ConfigEntryBase entryBase && seperatedEntryBase.Definition.Key == entryBase.Definition.Key)
                {
                    AccessTools.Property(fI.GetValue(this).GetType(), "Value").SetValue(fI.GetValue(this), seperatedEntryBase.BoxedValue);
                    Logging.Info(string.Concat("Identified and synced entry ", entryBase.Definition.Key));
                    return;
                }
            }

            Logging.Warning("Sync was called, however there are no identified entries to sync");
        }
    }
}
