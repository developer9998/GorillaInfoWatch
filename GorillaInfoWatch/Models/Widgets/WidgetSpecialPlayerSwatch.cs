using System;
using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetSpecialPlayerSwatch(NetPlayer player, float offset = 520, int scaleX = 80, int scaleY = 70) : WidgetSymbol(new Symbol(null)), IWidgetBehaviour
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
                image.enabled = false;
                if (!image.GetComponent<LayoutElement>())
                {
                    var element = image.gameObject.AddComponent<LayoutElement>();
                    element.ignoreLayout = true;
                    var rect_tform = image.GetComponent<RectTransform>();
                    rect_tform.anchoredPosition3D = rect_tform.anchoredPosition3D.WithX(offset).WithY(31.25f); // speaker hass x of 625
                    rect_tform.sizeDelta = new Vector2(scaleX, scaleY); // speaker is default i think, 120x100
                }
                if (Singleton<Main>.HasInstance)
                {
                    Predicate<NetPlayer> match = null;
                    foreach(var predicate in Singleton<Main>.Instance.SpecialSprites.Keys)
                    {
                        if (predicate(Player))
                        {
                            match = predicate;
                            break;
                        }
                    }
                    if (match != null && Singleton<Main>.Instance.Sprites.TryGetValue(Singleton<Main>.Instance.SpecialSprites[match], out var sprite))
                    {
                        image.enabled = true;
                        image.sprite = sprite;
                    }
                }
            }
        }

        public void InvokeUpdate()
        {
            if (!rig) return;

            if (!rig.isOfflineVRRig && !Utils.PlayerInRoom(Player.ActorNumber))
            {
                rig = null;
                Player = null;
                return;
            }
        }
    }
}