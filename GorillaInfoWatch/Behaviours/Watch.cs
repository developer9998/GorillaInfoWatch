using System;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class Watch : MonoBehaviour
    {
        public VRRig Rig;

        public float? TimeOffset;

        private Transform time_display, notification_display;

        private TMP_Text time_text, date_text;

        private AudioSource audio_device;

        private bool has_notification;

        private float notification_time;

        private MeshRenderer screen_renderer, outline_renderer;

        public bool HideWatch = false;

        public void Start()
        {
            audio_device = GetComponent<AudioSource>();

            time_display = transform.Find("Watch Head/Watch GUI/Time Display");
            notification_display = transform.Find("Watch Head/Watch GUI/Friend Display");
            time_text = transform.Find("Watch Head/Watch GUI/Time Display/Time").GetComponent<TMP_Text>();
            date_text = transform.Find("Watch Head/Watch GUI/Time Display/Day").GetComponent<TMP_Text>();

            screen_renderer = transform.Find("Watch Head/WatchScreen").GetComponent<MeshRenderer>();
            screen_renderer.material = new Material(screen_renderer.material);
            outline_renderer = transform.Find("Watch Head/WatchOuterScreen").GetComponent<MeshRenderer>();
            outline_renderer.material = new Material(outline_renderer.material);

            Rig.OnColorChanged += SetColour;
            SetColour(Rig.playerColor);

            transform.SetParent(Rig.leftHandTransform.parent, false);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;

            if (HideWatch)
            {
                time_text.enabled = false;
                date_text.enabled = false;
                transform.GetComponentsInChildren<MeshRenderer>(true).ForEach(renderer => renderer.enabled = false);
            }
        }

        public void OnDestroy()
        {
            Rig.OnColorChanged -= SetColour;
        }

        public void OnEnable()
        {
            InvokeRepeating(nameof(SetTime), 0f, 1f);
        }

        public void OnDisable()
        {
            CancelInvoke(nameof(SetTime));
        }

        public void SetColour(Color colour)
        {
            Color.RGBToHSV(colour, out float h, out float s, out _);
            float v = 0.13f * Mathf.Clamp((s + 1) * 0.9f, 1, float.MaxValue);
            var screen_colour = Color.HSVToRGB(h, s, v);
            screen_renderer.material.color = screen_colour;
            outline_renderer.material.color = colour;
        }

        public void Notify(string content, float duration, bool upbeat = true)
        {
            GorillaTagger.Instance.StartVibration(true, 0.04f, 0.2f);

            TMP_Text text = notification_display.Find("Text").GetComponent<TMP_Text>();
            text.text = content;

            audio_device.PlayOneShot(upbeat ? Singleton<Main>.Instance.Sounds["FriendJoin"] : Singleton<Main>.Instance.Sounds["FriendLeave"]);
            time_display.gameObject.SetActive(false);
            notification_display.gameObject.SetActive(true);

            has_notification = true;
            notification_time = Time.realtimeSinceStartup + duration;
        }

        private void SetTime()
        {
            if (time_display.gameObject.activeSelf && TimeOffset.HasValue)
            {
                var time = DateTime.UtcNow + TimeSpan.FromMinutes(TimeOffset.Value);
                time_text.text = time.ToShortTimeString();
                date_text.text = time.ToLongDateString();
            }
        }

        public void FixedUpdate()
        {
            if (has_notification && Time.realtimeSinceStartup > notification_time)
            {
                has_notification = false;
                time_display.gameObject.SetActive(true);
                notification_display.gameObject.SetActive(false);
            }
        }
    }
}
