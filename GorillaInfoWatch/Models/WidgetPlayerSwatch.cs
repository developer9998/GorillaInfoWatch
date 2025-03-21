using GorillaExtensions;
using GorillaInfoWatch.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models
{
    public class WidgetPlayerSwatch(NetPlayer player) : WidgetSymbol(new Symbol(null)), IWidgetBehaviour
    {
        public NetPlayer Player = player;

        public GameObject game_object { get; set; }

        public bool PerformNativeMethods => false;

        private VRRig rig;

        private Image image;

        public void Initialize(GameObject gameObject)
        {
            rig = GorillaGameManager.StaticFindRigForPlayer(Player);
            image = gameObject.GetComponent<Image>();
            image.enabled = true;
            if (!image.GetComponent<LayoutElement>())
            {
                var element = image.gameObject.AddComponent<LayoutElement>();
                element.ignoreLayout = true;
                var rect_tform = image.GetComponent<RectTransform>();
                rect_tform.anchoredPosition3D = rect_tform.anchoredPosition3D.WithX(44).WithY(31.25f); // speaker hass x of 625
                rect_tform.sizeDelta = new Vector2(90, 90); // speaker is default i think, 120x100
            }
            Logging.Info($"PlayerSwatch for {Player.NickName}: {((bool)rig ? rig.name : "null")}");
        }

        public void InvokeUpdate()
        {
            if (!rig) return;

            if (!rig.isOfflineVRRig && !Utils.PlayerInRoom(Player.ActorNumber))
            {
                Logging.Info($"PlayerSwatch for {Player.NickName} will be shut off");
                rig = null;
                Player = null;
                image.enabled = false;
                return;
            }

            var scoreboard_material = rig.scoreboardMaterial;
            if (image.material != scoreboard_material) image.material = scoreboard_material;

            var player_colour = rig.materialsToChangeTo[0].color;
            if (image.color != player_colour) image.color = player_colour;
        }
    }
}