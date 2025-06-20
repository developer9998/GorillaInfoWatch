using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System;
using UnityEngine;
using UnityEngine.UI;
using GorillaInfoWatch.Extensions;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class PushButton : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        public Widget_PushButton Widget;

        [SerializeField, HideInInspector]
        private BoxCollider collider;

        [SerializeField, HideInInspector]
        private MeshRenderer renderer;

        [SerializeField, HideInInspector]
        private Gradient colour = Gradients.Button;

        [SerializeField, HideInInspector]
        private bool bumped;

        public static float PressTime;

        private float? currentValue = null;
        private float targetValue;

        [SerializeField, HideInInspector]
        private MaterialPropertyBlock matProperties;
        private readonly int matIndex = 1;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            renderer = GetComponent<MeshRenderer>();
            renderer.materials[matIndex] = new Material(renderer.materials[matIndex]);

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
                colour = Widget.Colour ?? Gradients.Button;

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

                if (GetComponentInChildren<Image>(true) is Image image)
                {
                    image.gameObject.SetActive(widget.Symbol is not null);
                    if (image.gameObject.activeSelf)
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
                currentValue = Mathf.MoveTowards(currentValue.GetValueOrDefault(targetValue), targetValue, Time.deltaTime / 0.2f);
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
