using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Attributes;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Credits")]
    public class CreditScreen : WatchScreen
    {
        public override string Title => "Credits";

        public override void OnScreenOpen()
        {
            Build();
        }

        public void Build()
        {
            PageBuilder = new();

            LineBuilder main_credits = new();
            main_credits.AddLine("<line-indent=3em><line-height=110%>Dev\n<size=40%>dev9998: Lead Developer, Designer", new WidgetSymbolSwatch(new(Main.Instance.Sprites[EDefaultSymbol.DevSprite])));
            main_credits.AddLine("<line-indent=3em><line-height=110%>Luna\n<size=40%>lunakittyyy: Lead Developer, Tester", new WidgetSymbolSwatch(new(Main.Instance.Sprites[EDefaultSymbol.KaylieSprite])));
            main_credits.AddLine("<line-indent=3em><line-height=110%>Staircase\n<size=40%>spiralingstaircases: Lead Artist", new WidgetSymbolSwatch(new(Main.Instance.Sprites[EDefaultSymbol.StaircaseSprite])));
            main_credits.AddLines(1);
            main_credits.AddLine("<line-indent=3em><line-height=110%>H4RNS\n<size=40%>Modeler", new WidgetSymbolSwatch(new(Main.Instance.Sprites[EDefaultSymbol.H4rnsSprite])));
            main_credits.AddLine("<line-indent=3em><line-height=110%>Astrid\n<size=40%>astridgt: Tester", new WidgetSymbolSwatch(new(Main.Instance.Sprites[EDefaultSymbol.AstridSprite])));
            main_credits.AddLine("<line-indent=3em><line-height=110%>Cyan\n<size=40%>cyangt: Tester", new WidgetSymbolSwatch(new(Main.Instance.Sprites[EDefaultSymbol.CyanSprite])));
            main_credits.AddLine("<line-indent=3em><line-height=110%>Deactivated\n<size=40%>knownperson: Tester", new WidgetSymbolSwatch(new(Main.Instance.Sprites[EDefaultSymbol.DeactivatedSprite])));

            PageBuilder.AddPage(lines: main_credits);
        }
    }
}