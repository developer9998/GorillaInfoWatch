using TMPro;
using UnityEngine;

#if PLUGIN
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Enumerations;
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

        private float _lastTime;

        private HandIndicator _touching;

        private float _timer;

        public void OnTriggerEnter(Collider other)
        {
            if (_lastTime > (Time.realtimeSinceStartup - 1f) || _touching != null) return;

            if (other.TryGetComponent(out HandIndicator handIndicator) && handIndicator.isLeftHand != InfoWatch.LocalWatch.InLeftHand)
            {
                _touching = handIndicator;
                _timer = 0f;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (_touching == null || other.gameObject != _touching.gameObject) return;

            _touching = null;
            _timer = 0f;
        }

        public void Update()
        {
            if (_touching == null) return;

            _timer += Time.unscaledDeltaTime;

            GorillaTagger.Instance.StartVibration(_touching.isLeftHand, GorillaTagger.Instance.tapHapticDuration, Time.unscaledDeltaTime);

            if (_timer < 0.25f) return;

            _lastTime = Time.realtimeSinceStartup;

            try
            {
                GorillaTagger.Instance.StartVibration(_touching.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(_touching.isLeftHand);
                handPlayer.PlayOneShot(Main.EnumToAudio[Sounds.widgetButton], 0.2f);

                ShortcutManager.Instance.ExcecuteShortcut(_shortcut);
            }
            catch
            {

            }

            _touching = null;
            _timer = 0f;
        }

        public void SetShortcut(Shortcut shortcut)
        {
            SetActive(shortcut != null);

            if (shortcut == null)
            {
                _shortcut = null;
                return;
            }

            _shortcut = shortcut;
            buttonText.text = shortcut.Name;
        }

        public void SetActive(bool active)
        {
            if (gameObject.activeSelf == active) return;
            gameObject.SetActive(active);
        }
#endif
    }
}
