using TMPro;
using UnityEngine;

#if PLUGIN
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Models.Shortcuts;
using HandIndicator = GorillaTriggerColliderHandIndicator;
#endif

namespace GorillaInfoWatch.Behaviours.UI
{
    public class ShortcutButton : MonoBehaviour
    {
        public TMP_Text buttonText;

        [Space]

        public MeshRenderer buttonRenderer;

        public int materialIndex;

#if PLUGIN
        public Shortcut Shortcut => _shortcut;

        private Shortcut _shortcut;

        private Material _material;

        private Gradient _buttonColour;

        private HandIndicator _touching;

        private float _timer;

        private bool _activated;

        private float _activationTime;

        private float _holdDuration = 0.35f;

        private float _activationInterval = 1f;

        public void Start()
        {
            _holdDuration = Configuration.ShortcutHoldDuration.Value;
            Configuration.ShortcutHoldDuration.SettingChanged += (_, _) => _holdDuration = Configuration.ShortcutHoldDuration.Value;

            _activationInterval = Configuration.ShortcutHoldDuration.Value;
            Configuration.ShortcutInterval.SettingChanged += (_, _) => _activationInterval = Configuration.ShortcutInterval.Value;

            Material[] materials = buttonRenderer.materials;
            _material = materials[materialIndex] = new Material(materials[materialIndex]);
            buttonRenderer.materials = materials;

            enabled = _shortcut != null;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (_activated || _touching != null) return;

            if (other.TryGetComponent(out HandIndicator handIndicator) && handIndicator.isLeftHand != Watch.LocalWatch.InLeftHand)
            {
                _touching = handIndicator;
                _timer = 0f;

                AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(_touching.isLeftHand);
                handPlayer.PlayOneShot(Main.EnumToAudio[Sounds.activationGeneric], 0.2f);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (_touching == null || other.gameObject != _touching.gameObject) return;

            _touching = null;
            _timer = 0f;

            _material.color = _buttonColour.Evaluate(0);
        }

        public void Update()
        {
            float time;

            if (_activated && _activationTime > (Time.realtimeSinceStartup - _activationInterval))
            {
                time = Mathf.Clamp01(_activationTime - (Time.realtimeSinceStartup - _activationInterval));
                _material.color = _buttonColour.Evaluate(Mathf.Clamp01(time * 2f));
            }
            else if (_activated)
            {
                _activated = false;
                _material.color = _buttonColour.Evaluate(0);
            }

            if (_touching == null) return;

            _timer += Time.unscaledDeltaTime;

            GorillaTagger.Instance.StartVibration(_touching.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 4f, Time.unscaledDeltaTime);

            time = Mathf.Clamp01(_timer / _holdDuration);
            _material.color = _buttonColour.Evaluate(Mathf.Clamp01((time * 2f) - 1f));

            if (time < 1f) return;

            _activationTime = Time.realtimeSinceStartup;

            try
            {
                _activated = true;

                GorillaTagger.Instance.StartVibration(_touching.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(_touching.isLeftHand);
                handPlayer.PlayOneShot(Main.EnumToAudio[Sounds.deactivation], 0.2f);

                ShortcutHandler.Instance.ExcecuteShortcut(_shortcut);

                if (_shortcut.HasState) _buttonColour = _shortcut.GetState() ? ColourPalette.Green : ColourPalette.Red;
            }
            catch
            {

            }

            _touching = null;
            _timer = 0f;
        }

        public void SetShortcut(Shortcut shortcut)
        {
            _shortcut = shortcut;

            enabled = _shortcut != null;
            SetActive(_shortcut != null);

            if (_shortcut == null) return;

            buttonText.text = _shortcut.Name;
            _buttonColour = _shortcut.HasState ? (_shortcut.GetState() ? ColourPalette.Green : ColourPalette.Red) : ColourPalette.Button;
        }

        public void SetActive(bool active)
        {
            if (gameObject.activeSelf == active) return;
            gameObject.SetActive(active);
        }
#endif
    }
}
