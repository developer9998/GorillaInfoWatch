using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if PLUGIN
using System.Linq;
using GorillaExtensions;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.StateMachine;
using System.Collections;
#endif

namespace GorillaInfoWatch.Behaviours
{
    [DisallowMultipleComponent]
    public class InfoWatch : MonoBehaviour
    {
        // Assets
        public Transform watchHeadTransform, watchCanvasTransform;
        public GameObject watchStrap;

        public GameObject idleMenu, messageMenu, redirectIcon;

        public TMP_Text timeText, messageText, redirectText;

        public Slider messageSlider;

        public AudioSource audioDevice;

        public MeshRenderer screenRenderer, rimRenderer;

#if PLUGIN

        // Assets (cont.)
        private Material screenMaterial, screenRimMaterial;

        // Ownership
        public static InfoWatch LocalWatch;
        public VRRig Rig;

        // Data
        public bool InLeftHand = true;
        public bool HideWatch = false;
        public float? TimeOffset;

        // Handling
        public StateMachine<Menu_StateBase> stateMachine;
        public Menu_Home home;

        public IEnumerator Start()
        {
            if (Rig is null) yield return new WaitUntil(() => Rig is not null);

            if (Rig.isOfflineVRRig || Rig.isLocal)
            {
                if (LocalWatch is not null && LocalWatch != this)
                {
                    Logging.Warning("Duplicate local watch detected! Removing duplicate");
                    Destroy(this);
                    yield break;
                }

                Logging.Message($"Local watch located: {transform.GetPath().TrimStart('/')}");
                LocalWatch = this;
            }

            watchHeadTransform.localEulerAngles = watchHeadTransform.localEulerAngles.WithZ(-91.251f);

            //idleMenu = head.Find("Watch GUI/IdleMenu").gameObject;

            //timeText = idleMenu.transform.Find("TimeDate").GetComponent<TMP_Text>();
            // fpsText = idleMenu.transform.Find("FPS").GetComponent<TMP_Text>();

            //messageMenu = head.Find("Watch GUI/MessageMenu").gameObject;

            //messageText = messageMenu.transform.Find("Message").GetComponent<TMP_Text>();
            //messageSlider = messageMenu.transform.Find("Slider").GetComponent<Slider>();

            //redirectIcon = messageMenu.transform.Find("RedirectIcon").gameObject;
            //redirectText = messageMenu.transform.Find("RedirectText").GetComponent<TMP_Text>();

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

            //MeshRenderer screenRenderer = transform.Find("Watch Head/WatchScreen").GetComponent<MeshRenderer>();
            screenMaterial = new Material(screenRenderer.material);
            screenRenderer.material = screenMaterial;

            //MeshRenderer rimRenderer = transform.Find("Watch Head/WatchScreenRing").GetComponent<MeshRenderer>();
            screenRimMaterial = new Material(rimRenderer.material);
            rimRenderer.material = screenRimMaterial;

            Rig.OnColorChanged += SetColour;
            Events.OnRigSetLocallyInvisible += SetVisibilityCheck;

            Configure();
            yield break;
        }

        public void Configure()
        {
            transform.SetParent(InLeftHand ? Rig.leftHandTransform.parent : Rig.rightHandTransform.parent, false);
            transform.localPosition = InLeftHand ? Vector3.zero : new Vector3(0.01068962f, 0.040359f, -0.0006625927f);
            transform.localEulerAngles = InLeftHand ? Vector3.zero : new Vector3(-1.752f, 0.464f, 150.324f);
            transform.localScale = Vector3.one;

            SetVisibility(HideWatch || Rig.IsInvisibleToLocalPlayer);
            SetColour(Rig.playerColor);
        }

        public void OnDestroy()
        {
            Rig.OnColorChanged -= SetColour;
            Events.OnRigSetLocallyInvisible -= SetVisibilityCheck;
        }

        public void Update()
        {
            stateMachine?.Update();
        }

        public void SetVisibilityCheck(VRRig rig, bool invisible)
        {
            if (rig == Rig) SetVisibility(HideWatch || invisible);
        }

        public void SetVisibility(bool invisible)
        {
            watchHeadTransform.gameObject.SetActive(!invisible);
            watchStrap.GetComponentInChildren<MeshRenderer>(true).enabled = !invisible;
        }

        public void SetColour(Color playerColour)
        {
            screenRimMaterial.color = playerColour;
            Color.RGBToHSV(playerColour, out float H, out float S, out _);
            float V = 0.13f * Mathf.Clamp((S + 1) * 0.9f, 1, float.MaxValue);
            Color screenColour = Color.HSVToRGB(H, S, V);
            screenMaterial.color = screenColour;
        }

#endif
    }
}
