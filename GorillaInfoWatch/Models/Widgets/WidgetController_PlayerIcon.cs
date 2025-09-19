using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Significance;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    // anchor of 47.5
    public class WidgetController_PlayerIcon(NetPlayer player, float sizeDelta) : WidgetController
    {
        public override Type[] AllowedTypes => [typeof(Widget_Symbol)];
        public override bool? AllowModification => false;
        public override bool? UseBehaviour => true;
        public override float? Transform_ZPosition => 1f;

        public readonly NetPlayer Player = player;

        public readonly float SizeDelta = sizeDelta;

        private Image Image => (Widget as Widget_Symbol).image;

        private RigContainer playerRig;

        private Image monkeBase, monkeFace;

        private bool useMonkeSymbol;

        public WidgetController_PlayerIcon(NetPlayer player) : this(player, 70f)
        {
            // This is neccesary for providing the default value of the sizeDelta parameter of the "primary constructor"
            // Unfortunately, it does make the code a little more difficult to read, and there's probably a way around it, sorry!
        }

        public override void OnEnable()
        {
            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                Image.enabled = true;

                RectTransform rectTransform = Image.GetComponent<RectTransform>();
                rectTransform.sizeDelta = Vector2.one * SizeDelta;

                CreateSubSymbol(ref monkeBase, Symbols.TemplateHead);
                CreateSubSymbol(ref monkeFace, Symbols.TemplateFace);

                OnSignificanceChanged(Player, SignificanceManager.Instance.Significance.TryGetValue(Player, out PlayerSignificance[] significance) ? significance : null);

                playerRig.Rig.OnColorChanged += OnColourChanged;
                SignificanceManager.Instance.OnSignificanceChanged += OnSignificanceChanged;
            }
        }

        public override void OnDisable()
        {
            if (playerRig)
            {
                playerRig.Rig.OnColorChanged -= OnColourChanged;
                SignificanceManager.Instance.OnSignificanceChanged -= OnSignificanceChanged;
            }
        }


        private void CreateSubSymbol(ref Image image, Symbols watchSymbol)
        {
            string objectName = $"SubSymbol {watchSymbol}";

            if (Widget.Object && Widget.Object.transform.Find(objectName) is var child && child && child.TryGetComponent(out Image childImage))
            {
                image = childImage;
                return;
            }

            GameObject subObject = new(objectName);
            subObject.transform.SetParent(Widget.Object.transform);
            subObject.transform.localPosition = Vector3.zero;
            subObject.transform.localEulerAngles = Vector3.zero;
            subObject.transform.localScale = Vector3.one;
            subObject.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.one * SizeDelta;
            image = subObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.sprite = ((Symbol)watchSymbol).Sprite;
            image.color = Color.white;
        }

        public void OnSignificanceChanged(NetPlayer player, PlayerSignificance[] significance)
        {
            if (Player != player)
                return;

            if (significance != null && Array.Find(significance, item => item != null) is PlayerSignificance item && item.Symbol > Symbols.None)
            {
                useMonkeSymbol = false;
                Image.sprite = ((Symbol)item.Symbol).Sprite;
                Image.enabled = Image.sprite != null;
                monkeBase.enabled = false;
                monkeFace.enabled = false;
            }
            else
            {
                useMonkeSymbol = true;
                Image.enabled = false;
                monkeBase.enabled = true;
                monkeFace.enabled = true;
            }

            OnColourChanged(playerRig.Rig.playerColor);
        }

        public void OnColourChanged(Color colour)
        {
            if (useMonkeSymbol)
            {
                monkeBase.color = colour;
            }
        }
    }
}
