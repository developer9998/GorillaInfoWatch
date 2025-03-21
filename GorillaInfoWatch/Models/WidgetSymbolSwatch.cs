using GorillaExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models
{
    public class WidgetSymbolSwatch(Symbol symbol) : WidgetSymbol(symbol), IWidgetBehaviour
    {
        public GameObject game_object { get; set; }

        public bool PerformNativeMethods => true;

        private Image image;

        public void Initialize(GameObject gameObject)
        {
            image = gameObject.GetComponent<Image>();
            image.enabled = true;
            if (!image.GetComponent<LayoutElement>())
            {
                var element = image.gameObject.AddComponent<LayoutElement>();
                element.ignoreLayout = true;
                var rect_tform = image.GetComponent<RectTransform>();
                rect_tform.anchoredPosition3D = rect_tform.anchoredPosition3D.WithX(44).WithY(31.25f);
                // rect_tform.sizeDelta = new Vector2(90, 90);
            }
        }

        public void InvokeUpdate()
        {
            
        }
    }
}