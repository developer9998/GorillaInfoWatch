using GorillaExtensions;
using GorillaInfoWatch.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_PlayerSwatch(NetPlayer player, float offset = 520, int scaleX = 90, int scaleY = 90) : Widget_Symbol(new Models.Symbol(null))
    {
        public override bool AllowModification => false;
        public override bool UseBehaviour => true;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        private Material material;
        private Color colour;

        public override void Behaviour_Enable()
        {
            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                image.enabled = true;

                LayoutElement layoutElement = image.gameObject.GetOrAddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                RectTransform rectTransform = image.GetComponent<RectTransform>();
                rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.WithX(offset).WithY(31.25f);
                rectTransform.sizeDelta = new Vector2(scaleX, scaleY);

                SetSwatchColour();
            }
        }

        public override void Behaviour_Update()
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

            if (image.material != material) image.material = material;
            if (image.color != colour) image.color = colour;
        }
    }
}