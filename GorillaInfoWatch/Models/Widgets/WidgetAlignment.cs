using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public struct WidgetAlignment
    {
        public static WidgetAlignment Right = new(true);

        public static WidgetAlignment Left = new(false);

        public readonly string Classification;

        [Range(0, 100)]
        public float HorizontalAnchor;

        public float HorizontalOffset, DepthOffset;

        public WidgetAlignment(bool alignToRight)
        {
            Classification = alignToRight ? "right" : "left";
        }

        public WidgetAlignment(float anchor)
        {
            Classification = "custom";
            HorizontalAnchor = Mathf.Clamp(anchor, 0, 100);
        }
    }
}
