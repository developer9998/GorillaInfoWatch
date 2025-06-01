using System;
using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetSpecialPlayerSwatch(NetPlayer player, float offset = 520, int scaleX = 80, int scaleY = 70) : WidgetSymbol(new Symbol(null))
    {
        public override bool AllowModification => false;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        public override bool Init()
        {
            if (RigUtils.TryGetVRRig(Player, out playerRig))
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

                if (Singleton<Main>.HasInstance)
                {
                    Predicate<NetPlayer> match = null;

                    foreach (var predicate in Singleton<Main>.Instance.SpecialSprites.Keys)
                    {
                        if (predicate(Player))
                        {
                            match = predicate;
                            break;
                        }
                    }

                    if (match is not null && Singleton<Main>.Instance.Sprites.TryGetValue(Singleton<Main>.Instance.SpecialSprites[match], out var sprite))
                    {
                        image.enabled = true;
                        image.sprite = sprite;
                    }
                }
            }

            return true;
        }

        public override void Update()
        {
            if (Player is not null && playerRig is not null && playerRig.Creator != Player)
            {
                playerRig = null;
                image.enabled = false;
            }
        }
    }
}