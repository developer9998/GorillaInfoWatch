using BepInEx.Configuration;
using UnityEngine;

namespace GorillaInfoWatch.Tools;

public class Configuration
{
    public static ConfigEntry<Color> BackgroundColour;

    public static ConfigEntry<bool> ShowPublic, ShowPrivate, ShowSensitiveData;

    public Configuration(ConfigFile file)
    {
        file.SaveOnConfigSet = true;

        BackgroundColour = file.Bind("Appearance", "Background Colour", (Color)new Color32(69, 82, 87, 191),
                "The background colour of the watch menu");

        ShowPublic = file.Bind("Privacy", "Show Public Name", true,
                "Whether room names under public visibility are shown");

        ShowPrivate = file.Bind("Privacy", "Show Private Name", false,
                "Whether room names under private visibility are shown");

        ShowSensitiveData = file.Bind("Privacy", "Show Sensitive Data", false,
                "Whether sensitive data of players are shown");
    }
}