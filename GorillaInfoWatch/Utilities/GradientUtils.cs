using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Utilities
{
    public class GradientUtils
    {
        public static Gradient FromColour(params List<Color> colours)
        {
            if (colours == null)
                throw new ArgumentNullException(nameof(colours));

            if (colours.Count == 0 || colours.Count > 8)
                throw new ArgumentOutOfRangeException(nameof(colours));

            if (colours.Count == 1)
                colours.Add(colours.First());

            Gradient gradient = new();

            List<GradientColorKey> colourKeys = [];
            List<GradientAlphaKey> alphaKeys = [];

            for (int i = 0; i < colours.Count; i++)
            {
                float time = i / (colours.Count - 1);
                colourKeys.Add(new(colours[i], time));
                alphaKeys.Add(new(colours[i].a, time));
                // Logging.Info($"Gradient {colours[i]} at {time}");
            }

            gradient.SetKeys([.. colourKeys], [.. alphaKeys]);

            return gradient;
        }
    }
}
