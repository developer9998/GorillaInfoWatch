using GorillaInfoWatch.Utilities;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Gradients
    {
        public static readonly Gradient Button = GradientUtils.FromColour(new Color32(191, 188, 170, 255), new Color32(132, 131, 119, 255));

        public static readonly Gradient Green = GradientUtils.FromColour(new Color32(67, 188, 84, 255), new Color32(41, 162, 58, 255));

        public static readonly Gradient Red = GradientUtils.FromColour(new Color32(188, 67, 67, 255), new Color32(171, 50, 50, 255));

        public static readonly Gradient Blue = GradientUtils.FromColour(new Color32(40, 117, 215, 255), new Color32(14, 91, 189, 255));

        public static readonly Gradient Yellow = GradientUtils.FromColour(new Color32(215, 211, 40, 255), new Color32(189, 185, 14, 255));
    }
}
