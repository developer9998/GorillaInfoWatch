using GorillaInfoWatch.Behaviours;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models
{
    public class WidgetSymbol(Symbol symbol) : IWidget, IWidgetObject
    {
        public EWidgetType WidgetType => EWidgetType.Symbol;

        public Symbol Value = symbol;

        public int ObjectFits(GameObject game_object)
        {
            if (game_object && game_object.name == "Symbol" && game_object.TryGetComponent(out Image image))
            {
                return (image.sprite == Value.Sprite && image.material == Value.Material && image.color == Value.Colour) ? 2 : 1;
            }

            return 1;
        }

        public void CreateObject(MenuLine line, out GameObject game_object)
        {
            game_object = Object.Instantiate(line.Symbol, line.Symbol.transform.parent);
            game_object.name = "Symbol";

            game_object.SetActive(true);
            if (game_object.TryGetComponent(out Image image)) image.enabled = true;
        }

        public void ModifyObject(GameObject game_object)
        {
            if (game_object.TryGetComponent(out Image image))
            {
                image.sprite = Value.Sprite;
                image.material = Value.Material;
                image.color = Value.Colour;
            }
        }
    }
}