using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard), nameof(GorillaScoreBoard.RedrawPlayerLines))]
    public class ScoreboardPatch
    {
        public static bool Prefix(GorillaScoreBoard __instance)
        {
            __instance.boardText.supportRichText = true;
            __instance.boardText.text = __instance.GetBeginningString();
            __instance.buttonText.text = "";
            for (int i = 0; i < __instance.lines.Count; i++)
            {
                try
                {
                    if (!__instance.lines[i].gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    __instance.lines[i].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, __instance.startingYValue - __instance.lineHeight * i, 0f);
                    if (__instance.lines[i].linePlayer == null)
                    {
                        continue;
                    }

                    Text text = __instance.boardText;
                    text.text = text.text + "\n " + string.Concat("<color=#", ColorUtility.ToHtmlStringRGB(__instance.lines[i].playerVRRig.playerText.color), ">", __instance.NormalizeName(doIt: true, __instance.lines[i].linePlayer.NickName), "</color>");
                    if (__instance.lines[i].linePlayer != PhotonNetwork.LocalPlayer)
                    {
                        if (__instance.lines[i].reportButton.isActiveAndEnabled)
                        {
                            __instance.buttonText.text += "MUTE                                REPORT\n";
                        }
                        else
                        {
                            __instance.buttonText.text += "MUTE                HATE SPEECH    TOXICITY      CHEATING      CANCEL\n";
                        }
                    }
                    else
                    {
                        __instance.buttonText.text += "\n";
                    }
                }
                catch
                {
                }
            }

            return false;
        }
    }
}
