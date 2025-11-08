using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen, PreserveScreenSection]
    internal class CreditScreen : InfoScreen
    {
        public override string Title => "Credits";
        public override string Description => $"Credits for {Constants.Name} {Constants.Version}";

        private readonly string creditFormat = "<line-height=45%>{0}<br><size=60%>{1}: {2}";

        private PageBuilder pageBuilder;

        public override void OnScreenLoad()
        {
            if (pageBuilder == null)
            {
                pageBuilder = new();

                // TODO: load credit data from player significance

                LineBuilder baseCredits = new();

                baseCredits.Add(string.Format(creditFormat, "Dev", "dev9998", "Creator and Developer"), new Widget_Symbol(new(Symbols.Dev))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "Gizmo", "gizmogoat", "Creator and Developer"), new Widget_Symbol(new(Symbols.Gizmo))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "Cresmondo", "crescent.mondo", "2D Artist"), new Widget_Symbol(new(Symbols.Cresmondo))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "H4RNS", "hr4ns", "3D Artist"), new Widget_Symbol(new(Symbols.H4RNS))
                {
                    Alignment = WidgetAlignment.Left
                });

                baseCredits.Skip();

                baseCredits.Add(string.Format(creditFormat, "Astrid", "astridgt", "Tester"), new Widget_Symbol(new(Symbols.Astrid))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "Cyan", "cyangt", "Tester"), new Widget_Symbol(new(Symbols.Cyan))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "Deactivated", "knownperson", "Tester"), new Widget_Symbol(new(Symbols.Deactivated))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "Will", "will_0x40", "Tester"), new Widget_Symbol(new(Symbols.Maple))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "Lapis", "lapisgit", "Tester"), new Widget_Symbol(new(Symbols.Lapis))
                {
                    Alignment = WidgetAlignment.Left
                });
                baseCredits.Add(string.Format(creditFormat, "Kronicahl", "kronicahl", "Tester"), new Widget_Symbol(new(Symbols.Kronicahl))
                {
                    Alignment = WidgetAlignment.Left
                });
                pageBuilder.Add(baseCredits);

                LineBuilder supporterCredits = new();
                supporterCredits.Add(string.Format(creditFormat, "CBigback", "cbigbomb", "Supporter since March 9, 2025"), new Widget_Symbol(new(Symbols.Patreon))
                {
                    Alignment = WidgetAlignment.Left
                });
                supporterCredits.Add(string.Format(creditFormat, "Koda", "kodagtt", "Supporter since April 6, 2025"), new Widget_Symbol(new(Symbols.KoFi))
                {
                    Alignment = WidgetAlignment.Left
                });
                supporterCredits.Add(string.Format(creditFormat, "Guy", "saul15.sgma", "Supporter since May 19, 2025"), new Widget_Symbol(new(Symbols.Patreon))
                {
                    Alignment = WidgetAlignment.Left
                });
                supporterCredits.Add(string.Format(creditFormat, "iceyonly yapps", "icey_yapps", "Supporter since June 30, 2025"), new Widget_Symbol(new(Symbols.KoFi))
                {
                    Alignment = WidgetAlignment.Left
                });
                pageBuilder.Add("Supporters", supporterCredits);
            }
        }

        public override InfoContent GetContent() => pageBuilder;
    }
}