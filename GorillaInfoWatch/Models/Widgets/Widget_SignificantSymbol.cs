using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models.Significance;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_SignificantSymbol(NetPlayer player, float offset = 520, int scaleX = 80, int scaleY = 70) : Widget_Symbol(new Models.Symbol(null))
    {
        public override bool AllowModification => false;
        public override bool UseBehaviour => true;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        public override void Behaviour_Enable()
        {
            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                image.enabled = false;

                if (image.GetComponent<LayoutElement>() is null)
                {
                    LayoutElement layoutElement = image.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;

                    RectTransform rectTransform = image.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.WithX(offset).WithY(31.25f); // speaker hass x of 625
                    rectTransform.sizeDelta = new Vector2(scaleX, scaleY); // speaker is default i think, 120x100
                }

                if (Main.Instance.Significance.TryGetValue(Player, out PlayerSignificance significance) && Singleton<Main>.Instance.Sprites.TryGetValue(significance.Symbol, out Sprite sprite))
                {
                    image.enabled = true;
                    image.sprite = sprite;
                }
            }
        }

        public override void Behaviour_Update()
        {
            if (Player is not null && playerRig is not null && playerRig.Creator != Player)
            {
                playerRig = null;
                image.enabled = false;
            }
        }
    }
}