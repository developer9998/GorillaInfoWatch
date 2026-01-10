using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using System;
using System.Linq;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets;

public sealed class Widget_Symbol : Widget_Base
{
    public override bool Modification => Settings != null;
    public override float Depth => 3f;

    public readonly Symbol Settings;

    internal Image image;

    private static readonly object[] emptyControllerParams = [];

    internal Widget_Symbol()
    {
        // Used alongside WidgetControllers
    }

    public Widget_Symbol(Symbol settings)
    {
        Settings = settings;
    }

    public override void Initialize(PanelLine menuLine)
    {
        if (Object.Null())
        {
            Object = UnityEngine.Object.Instantiate(menuLine.Symbol, menuLine.Symbol.transform.parent);
            Object.name = "Symbol";
            Object.SetActive(true);
        }

        Object.TryGetComponent(out image);
    }

    public override void Modify()
    {
        if (image.Null() && Object.Exists() && !Object.TryGetComponent(out image))
        {
            // Logging.Info("NO");
            return;
        }

        image.enabled = true;
        image.sprite = Settings.Sprite;
        image.material = Settings.Material;
        image.color = Settings.Colour;
    }

    public override bool Equals(Widget_Base widget)
    {
        if (widget is null || widget is not Widget_Symbol widgetSymbol) return false;

        object[] controllerParmas = ControllerParameters ?? emptyControllerParams;
        return ControllerType == widgetSymbol.ControllerType && controllerParmas.SequenceEqual(widgetSymbol.ControllerParameters ?? emptyControllerParams);
    }
}