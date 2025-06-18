using GorillaInfoWatch.Behaviours.UI;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_Symbol(Symbol symbol) : Widget_Base
    {
        public Symbol Value = symbol;

        internal Image image;

        public override void Object_Construct(InfoWatchLine menuLine)
        {
            if (gameObject is null && !gameObject)
            {
                gameObject = UnityEngine.Object.Instantiate(menuLine.Symbol, menuLine.Symbol.transform.parent);
                gameObject.name = "Symbol";
                gameObject.SetActive(true);
            }

            if (gameObject.TryGetComponent(out image))
                image.enabled = true;
        }

        public override void Object_Modify()
        {
            if (image || (gameObject && gameObject.TryGetComponent(out image)))
            {
                image.sprite = Value.Sprite;
                image.material = Value.Material;
                image.color = Value.Colour;
            }
        }

        public override bool Equals(Widget_Base widget)
        {
            if (widget is null)
                return false;

            if (widget is not Widget_Symbol widgetSymbol)
                return false;

            return Value.Sprite == widgetSymbol.Value.Sprite && Value.Material == widgetSymbol.Value.Material && Value.Colour == widgetSymbol.Value.Colour;
        }
    }
}