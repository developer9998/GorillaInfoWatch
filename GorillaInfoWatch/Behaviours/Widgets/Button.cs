﻿using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using System;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Widgets
{
    /// <summary>
    /// A push and toggle compatible button, used commonly alongside a WidgetButton, though can be used down to the OnPressed action
    /// </summary>
    public class Button : MonoBehaviour
    {
        public Action OnPressed, OnReleased;
        
        public WidgetButton Widget;

        private BoxCollider collider;
        private MeshRenderer renderer;
        private TMP_Text text;
        private Color colour, bumped_colour;

        private bool bumped, toggle;

        public static float PressTime;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            renderer = GetComponent<MeshRenderer>();
            renderer.materials[1] = new Material(renderer.materials[1]) { color = colour };

            text = GetComponentInChildren<TMP_Text>();

            colour = new Color32(191, 188, 170, 255);
            bumped_colour = new Color32(132, 131, 119, 255);
        }

        public void ApplyButton(WidgetButton widget)
        {
            if (widget != null && Widget == widget) return;

            // prepare transition
            if (Widget != null)
            {
                Widget.Value = false;
                bumped = false;
                toggle = false;
                renderer.materials[1].color = colour;
            }

            // apply transition
            Widget = widget;
            if (Widget != null)
            {
                gameObject.SetActive(true);
                if (text) text.text = "";
                OnPressed = () => Widget.Command?.Invoke(Widget.Value, Widget.Parameters ?? []);
                toggle = widget.ButtonType == WidgetButton.EButtonType.Switch;
                bumped = widget.Value;
                renderer.materials[1].color = bumped ? bumped_colour : colour;
                return;
            }

            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;
        }

        public void LateUpdate()
        {
            if (!toggle && bumped && Time.realtimeSinceStartup > (PressTime + 0.33f))
            {
                bumped = false;
                if (Widget != null) Widget.Value = false;
                renderer.materials[1].color = colour;
                OnReleased?.Invoke();
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && Time.realtimeSinceStartup > (PressTime + 0.33f))
            {
                PressTime = Time.realtimeSinceStartup;
                
                // base functionality
                bumped = !toggle || (!bumped);
                renderer.materials[1].color = bumped ? bumped_colour : colour;
                Singleton<Main>.Instance.PressButton(this, component.isLeftHand);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                // additional func
                if (Widget != null)
                {
                    Widget.Value = bumped;
                }
                OnPressed?.Invoke(); 
            }
        }

        #if DEBUG

        public void PushButton()
        {
            try
            {
                OnPressed?.Invoke();
            }
            catch(Exception ex)
            {
                Logging.Error(GetComponentInParent<MenuLine>().name);
                Logging.Fatal(ex);
            }
        }

        #endif
    }
}
