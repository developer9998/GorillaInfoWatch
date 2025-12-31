using GorillaInfoWatch.Models;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    internal class VersionWarningScreen : InfoScreen
    {
        public override string Title => "GorillaInfoWatch is outdated!";

        public static string LatestVersion;

        public override InfoContent GetContent()
        {
            LineBuilder lines = new();

            lines.AppendLine("Your installation of GorillaInfoWatch is outdated:");
            lines.Append("Latest: ").AppendColour(LatestVersion, Color.green).AppendLine();
            lines.Append("Installed: ").AppendColour(Constants.Version, Color.red).AppendLine().Skip();

            lines.Add("To continue using GorillaInfoWatch, follow these instructions:");
            lines.Add("1. Navigate to the \"Exclusive Content\" category in the \"Dev's Dungeon\" Discord server", LineOptions.Wrapping);
            lines.Add("2. Find the dedicated thread for the mod in the \"exclusive-mods\" thread channel", LineOptions.Wrapping);
            lines.Add("3. Download and install the \"GorillaInfoWatch.dll\" file found in the channel", LineOptions.Wrapping);
            lines.Add("4. Restart Gorilla Tag if your installation is still actively being open", LineOptions.Wrapping);

            return lines;
        }
    }
}
