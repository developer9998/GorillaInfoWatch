using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public static class ColourPalette
    {
        public static readonly Gradient Button = CreatePalette(new Color32(191, 188, 170, 255), new Color32(132, 131, 119, 255));

        public static readonly Gradient Green = CreatePalette(new Color32(67, 188, 84, 255), new Color32(41, 162, 58, 255));

        public static readonly Gradient Red = CreatePalette(new Color32(188, 67, 67, 255), new Color32(171, 50, 50, 255));

        public static readonly Gradient Blue = CreatePalette(new Color32(40, 117, 215, 255), new Color32(14, 91, 189, 255));

        public static readonly Gradient Yellow = CreatePalette(new Color32(215, 211, 40, 255), new Color32(189, 185, 14, 255));

        public static readonly Gradient White = CreatePalette(new Color32(243, 243, 243, 255), new Color32(217, 217, 217, 255));

        public static readonly Gradient Black = CreatePalette(new Color32(73, 73, 73, 255), new Color32(48, 48, 48, 255));

        public static readonly Gradient Magenta = CreatePalette(new Color32(246, 9, 213, 255), new Color32(220, 0, 187, 255));

        public static Gradient CreatePalette(params List<Color> colours)
        {
            if (colours == null) throw new ArgumentNullException(nameof(colours));
            if (colours.Count == 0 || colours.Count > 8) throw new ArgumentOutOfRangeException(nameof(colours));

            if (colours.Count == 1) colours.Add(colours.First());

            Gradient gradient = new()
            {
                mode = GradientMode.PerceptualBlend
            };

            List<GradientColorKey> colourKeys = [];
            List<GradientAlphaKey> alphaKeys = [];

            for (int i = 0; i < colours.Count; i++)
            {
                float time = i / (colours.Count - 1);
                colourKeys.Add(new(colours[i], time));
                alphaKeys.Add(new(colours[i].a, time));
            }

            gradient.SetKeys([.. colourKeys], [.. alphaKeys]);

            return gradient;
        }
    }
}
