using GorillaExtensions;
using GorillaInfoWatch.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetPlayerSwatch(NetPlayer player, float offset = 520, int scaleX = 90, int scaleY = 90) : WidgetSymbol(new Models.Symbol(null))
    {
        public override bool AllowModification => false;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        public override bool Init()
        {
            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                image.enabled = true;

                if (image.GetComponent<LayoutElement>() is null)
                {
                    LayoutElement layoutElement = image.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;

                    RectTransform rectTransform = image.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.WithX(offset).WithY(31.25f);
                    rectTransform.sizeDelta = new Vector2(scaleX, scaleY);
                }

                SetSwatchColour();
            }

            return true;
        }

        public override void Update()
        {
            if (Player is not null && playerRig is not null && playerRig.Creator != Player)
            {
                Logging.Info($"PlayerSwatch for {Player.NickName} will be shut off");
                playerRig = null;
                return;
            }

            if (playerRig is not null)
                SetSwatchColour();
        }

        public void SetSwatchColour()
        {
            var scoreboard_material = playerRig.Rig.scoreboardMaterial;
            if (image.material != scoreboard_material) image.material = scoreboard_material;

            var player_colour = playerRig.Rig.playerColor;
            if (image.color != player_colour) image.color = player_colour;
        }
    }
}