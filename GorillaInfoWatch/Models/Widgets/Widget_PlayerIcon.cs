using GorillaExtensions;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models.Significance;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_PlayerIcon(NetPlayer player, float anchor, Vector2 size) : Widget_Symbol(new Symbol(null))
    {
        public override bool AllowModification => false;
        public override bool UseBehaviour => true;

        public NetPlayer Player = player;

        private RigContainer playerRig;

        private Image monkeBase, monkeFace;

        private bool useMonkeSymbol;

        public override void Behaviour_Enable()
        {
            if (VRRigCache.Instance.TryGetVrrig(Player, out playerRig))
            {
                image.enabled = true;

                LayoutElement layoutElement = image.gameObject.GetOrAddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                RectTransform rectTransform = image.GetComponent<RectTransform>();
                rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.WithX(anchor).WithY(31.25f); // speaker hass x of 625
                rectTransform.sizeDelta = size; // speaker is default i think, 120x100
                rectTransform.localPosition = rectTransform.localPosition.WithZ(-2);

                CreateSubSymbol(ref monkeBase, InfoWatchSymbol.TemplateHead);
                CreateSubSymbol(ref monkeFace, InfoWatchSymbol.TemplateFace);

                OnSignificanceChanged(Player, Main.Significance.TryGetValue(Player, out PlayerSignificance significance) ? significance : null);

                playerRig.Rig.OnColorChanged += OnColourChanged;
                Events.OnSignificanceChanged += OnSignificanceChanged;
            }
        }

        private void CreateSubSymbol(ref Image image, InfoWatchSymbol watchSymbol)
        {
            string objectName = $"SubSymbol {watchSymbol}";

            if (gameObject && gameObject.transform.Find(objectName) is var child && child && child.TryGetComponent(out Image childImage))
            {
                image = childImage;
                return;
            }

            GameObject subObject = new(objectName);
            subObject.transform.SetParent(gameObject.transform);
            subObject.transform.localPosition = Vector3.zero;
            subObject.transform.localEulerAngles = Vector3.zero;
            subObject.transform.localScale = Vector3.one;
            subObject.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.one * 80;
            image = subObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.sprite = ((Symbol)watchSymbol).Sprite;
            image.color = Color.white;
        }

        public override void Behaviour_Disable()
        {
            if (playerRig)
            {
                playerRig.Rig.OnColorChanged -= OnColourChanged;
                Events.OnSignificanceChanged -= OnSignificanceChanged;
            }
        }

        public void OnSignificanceChanged(NetPlayer player, PlayerSignificance significance)
        {
            if (Player != player)
                return;

            if (significance != null)
            {
                useMonkeSymbol = false;
                image.sprite = ((Symbol)significance.Symbol).Sprite;
                monkeBase.enabled = false;
                monkeFace.enabled = false;
            }
            else
            {
                useMonkeSymbol = true;
                image.enabled = false;
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