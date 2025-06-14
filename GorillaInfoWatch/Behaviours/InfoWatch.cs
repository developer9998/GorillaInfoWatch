using GorillaExtensions;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.StateMachine;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Behaviours
{
    [RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
    public class InfoWatch : MonoBehaviour
    {
        public static InfoWatch LocalWatch;

        public VRRig Rig;

        public string TimeZone;

        public float? TimeOffset;

        public GameObject idleMenu, messageMenu, redirectIcon;

        public TMP_Text timeText, fpsText, messageText, redirectText;

        public Slider messageSlider;

        public AudioSource AudioDevice;

        private Material screenMaterial, screenRimMaterial;

        public bool InLeftHand = true;

        public bool HideWatch = false;

        public StateMachine<Menu_StateBase> stateMachine;

        public Menu_Home home;

        public void Start()
        {
            if (Rig is null)
                Destroy(this);
            else if (Rig.isOfflineVRRig)
                LocalWatch = this;

            AudioDevice = GetComponent<AudioSource>();

            Transform head = transform.Find("Watch Head");
            head.localEulerAngles = head.localEulerAngles.WithZ(-91.251f);

            idleMenu = head.Find("Watch GUI/IdleMenu").gameObject;

            timeText = idleMenu.transform.Find("TimeDate").GetComponent<TMP_Text>();
            fpsText = idleMenu.transform.Find("FPS").GetComponent<TMP_Text>();

            messageMenu = head.Find("Watch GUI/MessageMenu").gameObject;

            messageText = messageMenu.transform.Find("Message").GetComponent<TMP_Text>();
            messageSlider = messageMenu.transform.Find("Slider").GetComponent<Slider>();

            redirectIcon = messageMenu.transform.Find("RedirectIcon").gameObject;
            redirectText = messageMenu.transform.Find("RedirectText").GetComponent<TMP_Text>();

            idleMenu.SetActive(false);
            messageMenu.SetActive(false);

            stateMachine = new();
            home = new(this);
            stateMachine.SwitchState(home);

            MeshRenderer[] rendererArray = transform.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer meshRenderer in rendererArray)
            {
                Material[] uberMaterials = [.. meshRenderer.materials.Select(material => material.CreateUberShaderVariant())];
                meshRenderer.materials = uberMaterials;
            }

            MeshRenderer screenRenderer = transform.Find("Watch Head/WatchScreen").GetComponent<MeshRenderer>();
            screenMaterial = new Material(screenRenderer.material);
            screenRenderer.material = screenMaterial;

            MeshRenderer rimRenderer = transform.Find("Watch Head/WatchScreenRing").GetComponent<MeshRenderer>();
            screenRimMaterial = new Material(rimRenderer.material);
            rimRenderer.material = screenRimMaterial;

            Rig.OnColorChanged += SetColour;
            SetColour(Rig.playerColor);

            Events.OnSetInvisibleToLocalPlayer += SetVisibilityCheck;
            SetVisibility(HideWatch || Rig.IsInvisibleToLocalPlayer);

            ConfigureTransform();
        }

        public void ConfigureTransform()
        {
            transform.SetParent(InLeftHand ? Rig.leftHandTransform.parent : Rig.rightHandTransform.parent, false);
            transform.localPosition = InLeftHand ? Vector3.zero : new Vector3(0.01068962f, 0.040359f, -0.0006625927f);
            transform.localEulerAngles = InLeftHand ? Vector3.zero : new Vector3(-1.752f, 0.464f, 150.324f);
            transform.localScale = Vector3.one;
        }

        public void OnDestroy()
        {
            Rig.OnColorChanged -= SetColour;
            Events.OnSetInvisibleToLocalPlayer -= SetVisibilityCheck;
        }

        public void Update()
        {
            stateMachine?.Update();
        }

        public void SetVisibilityCheck(VRRig rig, bool invisible)
        {
            if (rig == Rig)
                SetVisibility(invisible);
        }

        public void SetVisibility(bool invisible)
        {
            transform.Find("Watch Head").gameObject.SetActive(!invisible);
            transform.Find("Hand Model").GetComponentInChildren<MeshRenderer>(true).enabled = !invisible;
        }

        public void SetColour(Color playerColour)
        {
            screenRimMaterial.color = playerColour;
            Color.RGBToHSV(playerColour, out float hue, out float saturation, out _);
            float value = 0.13f * Mathf.Clamp((saturation + 1) * 0.9f, 1, float.MaxValue);
            Color screenColour = Color.HSVToRGB(hue, saturation, value);
            screenMaterial.color = screenColour;
        }
    }
}
