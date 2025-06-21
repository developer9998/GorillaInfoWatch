using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class PushButton : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        public Widget_PushButton Widget;

        [SerializeField]
        private BoxCollider collider;

        [SerializeField]
        private MeshRenderer renderer;

        [SerializeField, HideInInspector]
        private Gradient colour = Gradients.Button;

        [SerializeField, HideInInspector]
        private bool bumped;

        public static float PressTime;

        private float? currentValue = null;
        private float targetValue;

        [SerializeField]
        private Image image;

        [SerializeField, HideInInspector]
        private MaterialPropertyBlock matProperties;
        private readonly int matIndex = 1;

        public void Awake()
        {
            if (!collider)
            {
                collider = GetComponent<BoxCollider>();
                collider.isTrigger = true;
                gameObject.SetLayer(UnityLayer.GorillaInteractable);
            }

            if (!renderer)
            {
                renderer = GetComponent<MeshRenderer>();
                renderer.materials[matIndex] = new Material(renderer.materials[matIndex]);
            }

            if (!image) image = GetComponentInChildren<Image>(true);

            matProperties = new MaterialPropertyBlock();
            matProperties.SetColor(ShaderProps._BaseColor, colour.Evaluate(currentValue.GetValueOrDefault(0f)));
            matProperties.SetColor(ShaderProps._Color, colour.Evaluate(currentValue.GetValueOrDefault(0f)));
            renderer.SetPropertyBlock(matProperties, matIndex);
        }

        public void AssignWidget(Widget_PushButton widget)
        {
            if (widget is not null)
            {
                Widget = widget;
                gameObject.SetActive(true);

                currentValue ??= targetValue;
                colour = widget.IsReadOnly ? GradientUtils.FromColour(colours: [.. widget.Colour.colorKeys.Select(key => key.color).Select(colour =>
                {
                    Color.RGBToHSV(colour, out float h, out float s, out float v);
                    return Color.HSVToRGB(h, s * 0.4f, v);
                })]) : widget.Colour;

                OnPressed = delegate ()
                {
                    Widget.Command?.Invoke(Widget.Parameters ?? []);
                };

                if (!currentValue.HasValue || currentValue.Value == targetValue)
                {
                    matProperties.SetColor(ShaderProps._BaseColor, colour.Evaluate(currentValue.GetValueOrDefault(0f)));
                    matProperties.SetColor(ShaderProps._Color, colour.Evaluate(currentValue.GetValueOrDefault(0f)));
                    renderer.SetPropertyBlock(matProperties, matIndex);
                }

                if (image)
                {
                    bool hasSymbol = widget.Symbol != null;
                    image.gameObject.SetActive(hasSymbol);
                    if (hasSymbol)
                    {
                        image.sprite = widget.Symbol.Sprite;
                        image.material = widget.Symbol.Material;
                        image.color = widget.Symbol.Colour;
                    }
                }

                return;
            }

            Widget = null;
            currentValue = null;
            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;
        }

        public void OnDisable()
        {
            currentValue = null;
            targetValue = 0;
        }

        public void Update()
        {
            if (!currentValue.HasValue || currentValue.Value != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue.GetValueOrDefault(targetValue), targetValue, Time.deltaTime / 0.05f);
                float animatedValue = Mathf.Clamp01(AnimationCurves.EaseInSine.Evaluate(currentValue.Value));
                matProperties.SetColor(ShaderProps._BaseColor, colour.Evaluate(animatedValue));
                matProperties.SetColor(ShaderProps._Color, colour.Evaluate(animatedValue));
                renderer.SetPropertyBlock(matProperties, matIndex);
            }

            if (bumped && Time.realtimeSinceStartup > PressTime)
            {
                bumped = false;
                targetValue = 0f;
                OnReleased?.SafeInvoke();
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (Widget != null && Widget.IsReadOnly)
                return;

            if (PressTime > Time.realtimeSinceStartup || !collider.TryGetComponent(out HandIndicator component) || component.isLeftHand == InfoWatch.LocalWatch.InLeftHand)
                return;

            PressTime = Time.realtimeSinceStartup + 0.25f;

            bumped = true;
            targetValue = 1f;
            currentValue = 1f;
            matProperties.SetColor(ShaderProps._BaseColor, colour.Evaluate(1f));
            matProperties.SetColor(ShaderProps._Color, colour.Evaluate(1f));
            renderer.SetPropertyBlock(matProperties, matIndex);

            Singleton<Main>.Instance.PressButton(this, component.isLeftHand);
            GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            OnPressed?.SafeInvoke();
        }
    }
}
