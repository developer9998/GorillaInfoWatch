using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using GorillaLibrary;
using MelonLoader;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen, PreserveSection]
    internal class ModListScreen : InfoScreen
    {
        public override string Title => "Mods";
        public override string Description => "View and inspect the list of your installed mods";

        private MelonMod[] stateSupportedMods, mods;

        public void Awake()
        {
            var melons = MelonBase.RegisteredMelons.ToList();

            var melonMods = melons.Where(melon => melon is MelonMod).Cast<MelonMod>();

            stateSupportedMods = [.. melonMods.Where(melon => melon is GorillaMod gm && gm.IsStateSupported).OrderBy(melon => melon.Info.Name)];

            mods = [.. stateSupportedMods
                .Concat(melonMods.Except(stateSupportedMods))
                .OrderBy(mod => mod.Info.Name)
                .OrderByDescending(mod => mod is GorillaMod gm ? gm.Categories.Sum(category => category.Entries.Count) : -1)
                .Where(mod => mod != Melon<InfoMelonMod>.Instance)];
        }

        public override InfoContent GetContent()
        {
            LineBuilder lines = new();

            for (int i = 0; i < mods.Length; i++)
            {
                MelonMod melonMod = mods[i];

                Widget_PushButton pushButton = new(OpenModInfo, melonMod)
                {
                    Colour = ColourPalette.Blue,
                    Symbol = Content.Shared.Symbols["Info"]
                };

                if (stateSupportedMods.Contains(melonMod) && melonMod is GorillaMod gm)
                {
                    lines.Append(melonMod.Info.Name).Append(": ").AppendColour(gm.Enabled ? "Enabled" : "Disabled", gm.Enabled ? ColourPalette.Green.Evaluate(0) : ColourPalette.Red.Evaluate(0)).Add(new Widget_Switch(gm.Enabled, ToggleMod, gm), pushButton);
                    continue;
                }

                lines.Add(melonMod.Info.Name, pushButton);
            }

            return lines;
        }

        private void ToggleMod(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is GorillaMod gm)
            {
                gm.Enabled = value;
                SetText();
            }
        }

        private void OpenModInfo(object[] args)
        {
            if (args.ElementAtOrDefault(0) is MelonMod info)
            {
                ModInspectorScreen.Mod = info;
                LoadScreen<ModInspectorScreen>();
            }
        }
    }
}
