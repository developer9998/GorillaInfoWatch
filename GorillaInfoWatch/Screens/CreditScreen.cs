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

        private readonly string creditFormat = "<line-indent=3em><line-height=45%>{0}<br><size=60%>{1}: {2}";

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            lines.Add(string.Format(creditFormat, "Dev", "dev9998", "Creator and Developer"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Dev)));
            lines.Add(string.Format(creditFormat, "Gizmo", "gizmogoat", "Creator and Developer"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Gizmo)));
            lines.Add(string.Format(creditFormat, "Cresmondo", "crescent.mondo", "2D Artist"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Cresmondo)));
            lines.Add(string.Format(creditFormat, "H4RNS", "hr4ns", "3D Artist"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.H4RNS)));

            lines.Skip();

            lines.Add(string.Format(creditFormat, "Astrid", "astridgt", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Astrid)));
            lines.Add(string.Format(creditFormat, "Cyan", "cyangt", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Cyan)));
            lines.Add(string.Format(creditFormat, "Deactivated", "knownperson", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Deactivated)));
            lines.Add(string.Format(creditFormat, "Will", "will_0x40", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Will)));
            lines.Add(string.Format(creditFormat, "Lapis", "lapisgit", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Lapis)));
            lines.Add(string.Format(creditFormat, "Kronicahl", "kronicahl", "Tester"), new Widget_AnchoredSymbol(new(InfoWatchSymbol.Kronicahl)));

            return lines;
        }
    }
}