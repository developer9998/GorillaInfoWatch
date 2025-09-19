using GorillaInfoWatch.Tools;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    // anchor of 47.5
    public class WidgetController_PlayerSwatch(NetPlayer player) : WidgetController
    {
        public override Type[] AllowedTypes => [typeof(Widget_Symbol)];
        public override bool? AllowModification => false;
        public override bool? UseBehaviour => true;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        private Material material;
        private Color colour;

        private Image Image => (Widget as Widget_Symbol).image;

        public override void OnEnable()
        {
            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                Image.enabled = true;

                RectTransform rectTransform = Image.GetComponent<RectTransform>();
                rectTransform.sizeDelta = Vector2.one * 90f;

                SetSwatchColour();
            }
        }

        public override void Update()
        {
            if (Player is not null && playerRig is not null && playerRig.Creator != Player)
            {
                Logging.Info($"PlayerSwatch for {Player.NickName} will be shut off");
                playerRig = null;
                return;
            }

            if (playerRig is not null) SetSwatchColour();
        }

        public void SetSwatchColour()
        {
            VRRig vrRig = playerRig.Rig;
            int setMatIndex = vrRig.setMatIndex;

            if (setMatIndex == 0)
            {
                material = vrRig.scoreboardMaterial;
                colour = vrRig.playerColor;
            }
            else
            {
                Material designatedMaterial = vrRig.materialsToChangeTo[setMatIndex];
                material = designatedMaterial;
                colour = designatedMaterial.color;
            }

            if (Image.material != material) Image.material = material;
            if (Image.color != colour) Image.color = colour;
        }
    }
}
