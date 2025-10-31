using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if PLUGIN
using System;
using System.Linq;
using GorillaExtensions;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Behaviours.Networking;
#endif

namespace GorillaInfoWatch.Behaviours
{
    [DisallowMultipleComponent]
    public class InfoWatch : MonoBehaviour
    {
        // Assets
        public Transform  watchHeadTransform, watchCanvasTransform;
        public GameObject watchStrap;

        public AudioSource audioDevice;

        public MeshRenderer screenRenderer, rimRenderer;

        [Header("Menu Interface")] public Animator menuAnimator;

        public string standardTrigger, mediaTrigger;

        [FormerlySerializedAs("idleMenu")] public GameObject homeMenu;

        public GameObject messageMenu;

        public GameObject redirectIcon;

        public TMP_Text timeText, messageText, redirectText;

        public Slider messageSlider;

        [Header("Home : Media Player")] public TMP_Text trackTitle;

        public TMP_Text trackAuthor;

        public TMP_Text trackElapsed;

        public TMP_Text trackRemaining;

        public RawImage trackThumbnail;

        public Slider trackProgression;

#if PLUGIN
        public bool IsLocalWatch => this == LocalWatch;

        // Assets (cont.)
        private Material screenMaterial, screenRimMaterial;

        // Ownership
        public static InfoWatch LocalWatch;
        public        VRRig     Rig;

        // Data
        public bool   InLeftHand = true;
        public bool   HideWatch;
        public float? TimeOffset;

        // Handling
        public StateMachine<Menu_StateBase> MenuStateMachine;
        public Menu_Home                    HomeState;

        private bool mediaTriggerState;

        public async void Start()
        {
            if (Rig is null) await new WaitWhile(() => Rig == null).AsAwaitable();

            if (Rig.isOfflineVRRig)
            {
                if (LocalWatch is not null && LocalWatch != this)
                {
                    Logging.Warning("Duplicate local watch detected to remove");
                    Destroy(this);

                    return;
                }

                Logging.Message("Local Watch");
                Logging.Info(transform.GetPath().TrimStart('/'));

                LocalWatch = this;
                MediaManager.Instance.OnSessionFocussed += OnSessionFocussed;
                MediaManager.Instance.OnMediaChanged += OnMediaChanged;
                MediaManager.Instance.OnTimelineChanged += OnTimelineChanged;
            }

            watchHeadTransform.localEulerAngles = watchHeadTransform.localEulerAngles.WithZ(-91.251f);

            homeMenu.SetActive(false);
            messageMenu.SetActive(false);

            MenuStateMachine = new StateMachine<Menu_StateBase>();
            HomeState = new Menu_Home(this);
            MenuStateMachine.SwitchState(HomeState);

            MeshRenderer[] rendererArray = transform.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer meshRenderer in rendererArray)
            {
                Material[] uberMaterials =
                        [.. meshRenderer.materials.Select(material => material.CreateUberShaderVariant()),];

                meshRenderer.materials = uberMaterials;
            }

            //MeshRenderer screenRenderer = transform.Find("Watch Head/WatchScreen").GetComponent<MeshRenderer>();
            screenMaterial = new Material(screenRenderer.material);
            screenRenderer.material = screenMaterial;

            //MeshRenderer rimRenderer = transform.Find("Watch Head/WatchScreenRing").GetComponent<MeshRenderer>();
            screenRimMaterial = new Material(rimRenderer.material);
            rimRenderer.material = screenRimMaterial;

            Rig.OnColorChanged += SetColour;
            Events.OnRigSetInvisibleToLocal += SetVisibilityCheck;

            ConfigureWatch();
        }

        public void ConfigureWatch()
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
            Events.OnRigSetInvisibleToLocal -= SetVisibilityCheck;
        }

        public void Update()
        {
            MenuStateMachine?.Update();
        }

#region Appearance

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
            Color.RGBToHSV(playerColour, out float H, out float S, out float _);
            float V = 0.13f * Mathf.Clamp((S + 1) * 0.9f, 1, float.MaxValue);
            Color screenColour = Color.HSVToRGB(H, S, V);
            screenMaterial.color = screenColour;
        }

#endregion

#region Media Player

        public void OnSessionFocussed(MediaManager.Session focussedSession)
        {
            if (!IsLocalWatch) return;

            if (focussedSession != null)
            {
                OnMediaChanged(focussedSession);
                OnTimelineChanged(focussedSession);

                return;
            }

            if (mediaTriggerState)
            {
                mediaTriggerState = false;
                menuAnimator.SetTrigger(standardTrigger);
            }
        }

        public void OnMediaChanged(MediaManager.Session session)
        {
            if (!IsLocalWatch || MediaManager.Instance.FocussedSession != session.Id) return;

            trackTitle.text = session.Title;
            trackAuthor.text = session.Artist;
            trackThumbnail.texture = session.Thumbnail;

            bool hasMedia = session.Title != null && session.Title.Length > 0;
            if (hasMedia != mediaTriggerState)
            {
                mediaTriggerState = hasMedia;
                string trigger = hasMedia ? mediaTrigger : standardTrigger;
                menuAnimator.SetTrigger(trigger);
            }

            if (mediaTriggerState)
            {
                NetworkManager.Instance.SetProperty("Title",  session.Title);
                NetworkManager.Instance.SetProperty("Artist", session.Artist);
                NetworkManager.Instance.SetProperty("Length", session.EndTime);
            }
            else
            {
                NetworkManager.Instance.RemoveProperty("Title");
                NetworkManager.Instance.RemoveProperty("Artist");
                NetworkManager.Instance.RemoveProperty("Length");
            }
        }

        public void OnTimelineChanged(MediaManager.Session session)
        {
            if (!IsLocalWatch || MediaManager.Instance.FocussedSession != session.Id) return;

            trackElapsed.text = TimeSpan.FromSeconds(session.Position).ToString(@"mm\:ss");
            trackRemaining.text = string.Concat("-",
                    TimeSpan.FromSeconds(session.EndTime - session.Position).ToString(@"mm\:ss"));

            trackProgression.value = session.EndTime > 0
                                             ? Mathf.Lerp(trackProgression.minValue, trackProgression.maxValue,
                                                     Convert.ToSingle(Math.Round(session.Position / session.EndTime,
                                                             3)))
                                             : trackProgression.minValue;
        }

#endregion

#endif
    }
}