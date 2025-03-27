using GorillaExtensions;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetPlayerSwatch(NetPlayer player, float offset = 520, int scaleX = 90, int scaleY = 90) : WidgetSymbol(new Symbol(null)), IWidgetBehaviour
    {
        public NetPlayer Player = player;

        public GameObject game_object { get; set; }

        public bool PerformNativeMethods => false;

        private VRRig rig;

        private Image image;

        public void Initialize(GameObject gameObject)
        {
            if (RigUtils.TryGetVRRig(Player, out var playerRig))
            {
                rig = playerRig.Rig;
                image = gameObject.GetComponent<Image>();
                image.enabled = true;
                if (!image.GetComponent<LayoutElement>())
                {
                    var element = image.gameObject.AddComponent<LayoutElement>();
                    element.ignoreLayout = true;
                    var rect_tform = image.GetComponent<RectTransform>();
                    rect_tform.anchoredPosition3D = rect_tform.anchoredPosition3D.WithX(offset).WithY(31.25f);
                    rect_tform.sizeDelta = new Vector2(scaleX, scaleY);
                }
                // Logging.Info($"PlayerSwatch for {Player.NickName}: {((bool)rig ? rig.name : "null")}");
                SetSwatchColour();
            }
        }

        public void InvokeUpdate()
        {
            if (!rig) return;

            if (!rig.isOfflineVRRig && !Utils.PlayerInRoom(Player.ActorNumber))
            {
                Logging.Info($"PlayerSwatch for {Player.NickName} will be shut off");
                rig = null;
                Player = null;
                return;
            }

            SetSwatchColour();
        }

        public void SetSwatchColour()
        {
            var scoreboard_material = rig.scoreboardMaterial;
            if (image.material != scoreboard_material) image.material = scoreboard_material;

            var player_colour = rig.materialsToChangeTo[0].color;
            if (image.color != player_colour) image.color = player_colour;
        }
    }
}