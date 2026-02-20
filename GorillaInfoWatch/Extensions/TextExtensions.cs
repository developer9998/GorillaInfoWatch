using System.Text;
using TMPro;
using UnityEngine.UI;

namespace GorillaInfoWatch.Extensions;

internal static class TextExtensions
{
    public static string[] GetArrayFromText(this TMP_Text component, string text, string prepend = null)
    {
        component.text = text;
        component.ForceMeshUpdate(true, true);
        CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(component);

        TMP_TextInfo textInfo = component.textInfo;

        string[] lines = new string[textInfo.lineCount];

        StringBuilder str = new();

        for (int i = 0; i < textInfo.lineCount; i++)
        {
            TMP_LineInfo lineInfo = textInfo.lineInfo[i];
            int startCharIndex = lineInfo.firstCharacterIndex;
            int endCharIndex = startCharIndex + lineInfo.characterCount;

            for (int charIndex = startCharIndex; charIndex < endCharIndex; charIndex++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
                str.Append(charInfo.character);
            }

            lines[i] = i != 0 || string.IsNullOrEmpty(prepend) ? str.ToString() : string.Concat(prepend, str.ToString());
            str.Clear();
        }

        return lines;
    }
}
