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

        // Customization
        public ConfigEntry<PresetColours> MenuColour, FavouriteColour;

        // Data
        public ConfigEntry<bool> TwFourHour;
        public ConfigEntry<int> RefreshRate;
        public ConfigEntry<float> ActivationVolume, ButtonVolume;

        public Configuration(BepInConfig config)
        {
            File = config.Config;

            MenuColour = File.Bind("Customization", "Menu Colour", PresetColours.Black, "The colour used to serve as the default background colour of the menu");
            FavouriteColour = File.Bind("Customization", "Favourite Colour", PresetColours.Yellow, "The colour used to serve as a unique identifier for those who you have favourited");

            // 100 hour clock gives me a headache and perhaps an aneurysm
            TwFourHour = File.Bind("Data", "24-Hour Time", false, "Determines whether the mod uses the 24-hour clock system, rather than 12-hour");

            RefreshRate = File.Bind("Data", "Refresh Rate", 4, "The amount of times the menu is refreshed each second");

            ActivationVolume = File.Bind("Data", "Activation Volume", 1f, "The volume of the activation sound indicator when opening/closing the menu");
            ButtonVolume = File.Bind("Data", "Button Volume", 1f, "The volume of the button sound indicator when pressing a menu button");
        }

        public void Sync(ConfigEntryBase seperatedEntryBase)
        {
            FieldInfo[] fieldInfo = GetType().GetFields();
            foreach (FieldInfo fI in fieldInfo)
            {
                if (fI.GetValue(this) is ConfigEntryBase entryBase && seperatedEntryBase.Definition.Key == entryBase.Definition.Key)
                {
                    // The ConfigEntry overrides the BoxedValue of the ConfigEntryBase with it's own property so we need to correctly set it here
                    AccessTools.Property(fI.GetValue(this).GetType(), "Value").SetValue(fI.GetValue(this), seperatedEntryBase.BoxedValue);
                    Logging.Info(string.Concat("Identified and synced entry ", entryBase.Definition.Key));
                    return;
                }
            }

            Logging.Warning("Sync was called, however there are no identified entries to sync");
        }
    }
}
