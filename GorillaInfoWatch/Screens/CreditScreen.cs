using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;

namespace GorillaInfoWatch.Screens
{
    [DisplayAtHomeScreen]
    public class CreditScreen : WatchScreen
    {
        public override string Title => "Credits";

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            lines.AddLine("<line-indent=3em><line-height=110%>Dev\n<size=40%>dev9998: Lead Developer, Designer", new WidgetSymbolSwatch(new(EDefaultSymbol.DevSprite)));
            lines.AddLine("<line-indent=3em><line-height=110%>Gizmo\n<size=40%>gizmogoat: Lead Developer, Tester", new WidgetSymbolSwatch(new(EDefaultSymbol.KaylieSprite)));
            lines.AddLine("<line-indent=3em><line-height=110%>Staircase\n<size=40%>spiralingstaircases: Lead Artist", new WidgetSymbolSwatch(new(EDefaultSymbol.StaircaseSprite)));
            lines.AddLines(1);
            lines.AddLine("<line-indent=3em><line-height=110%>H4RNS\n<size=40%>Modeler", new WidgetSymbolSwatch(new(EDefaultSymbol.H4rnsSprite)));
            lines.AddLines(1);
            lines.AddLine("<line-indent=3em><line-height=110%>Astrid\n<size=40%>astridgt: Tester", new WidgetSymbolSwatch(new(EDefaultSymbol.AstridSprite)));
            lines.AddLine("<line-indent=3em><line-height=110%>Cyan\n<size=40%>cyangt: Tester", new WidgetSymbolSwatch(new(EDefaultSymbol.CyanSprite)));
            lines.AddLine("<line-indent=3em><line-height=110%>Deactivated\n<size=40%>knownperson: Tester", new WidgetSymbolSwatch(new(EDefaultSymbol.DeactivatedSprite)));

            return lines;
        }
    }
}