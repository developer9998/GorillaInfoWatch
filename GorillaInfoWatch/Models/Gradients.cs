using GorillaInfoWatch.Utilities;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Gradients
    {
        public static readonly Gradient Button = GradientUtils.FromColour(new Color32(191, 188, 170, 255), new Color32(132, 131, 119, 255));

        public static readonly Gradient Green = GradientUtils.FromColour(new Color32(67, 188, 84, 255), new Color32(41, 162, 58, 255));

        public static readonly Gradient Red = GradientUtils.FromColour(new Color32(188, 67, 67, 255), new Color32(171, 50, 50, 255));
    }
}
