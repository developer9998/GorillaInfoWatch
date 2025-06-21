using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class CreditScreen : InfoWatchScreen
    {
        public override string Title => "Credits";
        public override string Description => $"Credits for {Constants.Name} v{Constants.Version}";

        private readonly string creditFormat = "<line-indent=4em><line-height=45%>{0}<br><size=60%>{1}: {2}";

        public override ScreenContent GetContent()
        {
            LineBuilder baseCredits = new();

            baseCredits.Add(string.Format(creditFormat, "Dev", "dev9998", "Creator and Developer"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Dev)));
            baseCredits.Add(string.Format(creditFormat, "Gizmo", "gizmogoat", "Creator and Developer"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Gizmo)));
            baseCredits.Add(string.Format(creditFormat, "Cresmondo", "crescent.mondo", "2D Artist"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Cresmondo)));
            baseCredits.Add(string.Format(creditFormat, "H4RNS", "hr4ns", "3D Artist"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.H4RNS)));

            baseCredits.Skip();

            baseCredits.Add(string.Format(creditFormat, "Astrid", "astridgt", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Astrid)));
            baseCredits.Add(string.Format(creditFormat, "Cyan", "cyangt", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Cyan)));
            baseCredits.Add(string.Format(creditFormat, "Deactivated", "knownperson", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Deactivated)));
            baseCredits.Add(string.Format(creditFormat, "Will", "will_0x40", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Will)));
            baseCredits.Add(string.Format(creditFormat, "Lapis", "lapisgit", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Lapis)));
            baseCredits.Add(string.Format(creditFormat, "Kronicahl", "kronicahl", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Kronicahl)));

            LineBuilder supporterCredits = new();

            supporterCredits.Add(string.Format(creditFormat, "CBigback", "cbigbomb", "Supporter since March 9, 2025"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Patreon)));

            supporterCredits.Add(string.Format(creditFormat, "Koda", "kodagtt", "Supporter since April 6, 2025"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.KoFi)));

            supporterCredits.Add(string.Format(creditFormat, "Guy", "saul15.sgma", "Supporter since May 19, 2025"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Patreon)));

            return new PageBuilder((string.Empty, baseCredits), ("Supporters", supporterCredits));
        }
    }
}