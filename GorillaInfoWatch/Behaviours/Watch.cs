using System;
using System.Collections;
using System.Linq;
using GorillaExtensions;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Behaviours
{
    [RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
    public class Watch : MonoBehaviour
    {
        public VRRig Rig;

        public string TimeZone;

        public float? TimeOffset;

        private GameObject idleMenu, messageMenu;

        private TMP_Text timeText, fpsText, messageText;

        private Slider messageSlider;

        public AudioSource AudioDevice;

        public bool HasNotification;

        private IEnumerator notificationRoutine;

        private MeshRenderer screen_renderer, outline_renderer;

        public bool InLeftHand = true;

        public bool HideWatch = false;

        private int frameCount;
        private float timeCount;

        public void Start()
        {
            AudioDevice = GetComponent<AudioSource>();

            Transform head = transform.Find("Watch Head");
            head.localEulerAngles = head.localEulerAngles.WithZ(-91.251f);

            idleMenu = head.Find("Watch GUI/IdleMenu").gameObject;

            timeText = idleMenu.transform.Find("TimeDate").GetComponent<TMP_Text>();
            fpsText = idleMenu.transform.Find("FPS").GetComponent<TMP_Text>();

            messageMenu = head.Find("Watch GUI/MessageMenu").gameObject;

            messageText = messageMenu.transform.Find("Message").GetComponent<TMP_Text>();
            messageSlider = messageMenu.transform.Find("Slider").GetComponent<Slider>();

            idleMenu.SetActive(true);
            messageMenu.SetActive(false);

            MeshRenderer[] rendererArray = transform.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer meshRenderer in rendererArray)
            {
                Material[] uberMaterials = [.. meshRenderer.materials.Select(material => material.CreateUberShaderVariant())];
                meshRenderer.materials = uberMaterials;
            }

            screen_renderer = transform.Find("Watch Head/WatchScreen").GetComponent<MeshRenderer>();
            screen_renderer.material = new Material(screen_renderer.material);
            outline_renderer = transform.Find("Watch Head/WatchScreenRing").GetComponent<MeshRenderer>();
            outline_renderer.material = new Material(outline_renderer.material);

            // sub
            Rig.OnColorChanged += SetColour;
            SetColour(Rig.playerColor);
            Events.OnSetInvisibleToLocalPlayer += SetVisibilityCheck;
            SetVisibility(HideWatch || Rig.IsInvisibleToLocalPlayer);

            transform.SetParent(Rig.leftHandTransform.parent, false);
            transform.localPosition = InLeftHand ? Vector3.zero : new Vector3(0.01068962f, 0.040359f, -0.0006625927f);
            transform.localEulerAngles = InLeftHand ? Vector3.zero : new Vector3(-1.752f, 0.464f, 150.324f);
            transform.localScale = Vector3.one;
        }

        public void OnDestroy()
        {
            // unsub
            Rig.OnColorChanged -= SetColour;
            Events.OnSetInvisibleToLocalPlayer -= SetVisibilityCheck;
        }

        public void Update()
        {
            if (timeCount < 1f)
            {
                timeCount += Time.unscaledDeltaTime;
                frameCount++;
                return;
            }

            InfrequentUpdate();
            timeCount = 0f;
            frameCount = 0;
        }

        private void InfrequentUpdate()
        {
            if (TimeOffset.HasValue)
            {
                DateTime dateTime = DateTime.UtcNow + TimeSpan.FromMinutes(TimeOffset.Value);
                string time = dateTime.ToShortTimeString();
                string date = dateTime.ToLongDateString();
                timeText.text = string.Format("<cspace=0.1em>{0}</cspace><br><size=50%>{1}</size>", time, date);
            }
            fpsText.text = $"FPS: {(Rig.isOfflineVRRig ? Mathf.FloorToInt((float)frameCount / timeCount) : Rig.fps)}";
        }

        public void SetVisibilityCheck(VRRig rig, bool invisible)
        {
            Logging.Info($"{rig.Creator.NickName} {invisible}");
            if (rig == Rig)
                SetVisibility(invisible);
        }

        public void SetVisibility(bool invisible)
        {
            transform.Find("Watch Head").gameObject.SetActive(!invisible);
            transform.Find("Hand Model").GetComponentInChildren<MeshRenderer>(true).enabled = !invisible;
        }

        public void SetColour(Color colour)
        {
            Color.RGBToHSV(colour, out float h, out float s, out _);
            float v = 0.13f * Mathf.Clamp((s + 1) * 0.9f, 1, float.MaxValue);
            var screen_colour = Color.HSVToRGB(h, s, v);
            screen_renderer.material.color = screen_colour;
            outline_renderer.material.color = colour;
        }

        public void DisplayMessage(string content, float duration, EWatchSound sound)
        {
            if (notificationRoutine != null)
                StopCoroutine(notificationRoutine);

            notificationRoutine = SetMessage(content, duration, sound);
            StartCoroutine(notificationRoutine);
        }

        public IEnumerator SetMessage(string content, float duration, EWatchSound sound)
        {
            HasNotification = true;
            idleMenu.SetActive(false);
            messageMenu.SetActive(true);
            messageText.text = content;
            messageSlider.value = 1f;

            if (Main.HasInstance && Main.Instance.Sounds.TryGetValue(sound, out AudioClip clip))
                AudioDevice.PlayOneShot(clip);
            GorillaTagger.Instance.StartVibration(true, 0.04f, 0.2f);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                float progress = Mathf.Clamp01(1 - (elapsed / duration)) * messageSlider.maxValue;
                messageSlider.value = messageSlider.wholeNumbers ? Mathf.CeilToInt(progress) : progress;
                yield return new WaitForFixedUpdate();
            }

            HasNotification = false;
            idleMenu.SetActive(true);
            messageMenu.SetActive(false);
        }
    }
}
