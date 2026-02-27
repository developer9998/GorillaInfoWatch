using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if PLUGIN
using System;
using GorillaInfoWatch.Tools;
using System.Linq;
using GorillaExtensions;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Behaviours.Networking;
#endif

namespace GorillaInfoWatch.Behaviours.UI
{
    [DisallowMultipleComponent]
    public class Watch : MonoBehaviour
    {
        // Assets
        public Transform watchHeadTransform, watchCanvasTransform;
        public GameObject watchStrap;

        public AudioSource audioDevice;

        public MeshRenderer screenRenderer, rimRenderer;

        [Header("Menu Interface")]

        public Animator menuAnimator;

        [FormerlySerializedAs("idleMenu")]
        public GameObject homeMenu;

        public GameObject messageMenu;

        public GameObject redirectIcon;

        public TMP_Text timeText, messageText, redirectText;

        public Slider messageSlider;

        public ShortcutButton shortcutButton;

        [Header("Media Controller")]

        public TMP_Text trackTitle;

        public TMP_Text trackAuthor;

        public TMP_Text trackElapsed;

        public TMP_Text trackRemaining;

        public Image trackThumbnail;

        public Slider trackProgression;

#if PLUGIN

        public bool IsLocalWatch => this == LocalWatch;

        // Assets (cont.)
        private Material screenMaterial, screenRimMaterial;

        // Ownership
        public static Watch LocalWatch;
        public VRRig Rig;

        // Data
        public bool InLeftHand = true;
        public bool HideWatch = false;
        public float? TimeOffset;

        // Handling
        public StateMachine<Menu_StateBase> MenuStateMachine;
        public Menu_Home HomeState;

        private WatchTab currentTab;

        private bool hasMediaSession = false;

        private CustomPushButton mediaNavigationButton;

        public async void Start()
        {
            if (Rig is null) await new WaitWhile(() => Rig == null).AsAwaitable();

            if (Rig.isOfflineVRRig)
            {
                ConfigureWatchLocal();
            }
            else
            {
                shortcutButton.SetActive(false);
            }

            menuAnimator.SetBool(Constants.AnimatorProperty_IsLocal, IsLocalWatch);

            watchHeadTransform.localEulerAngles = watchHeadTransform.localEulerAngles.WithZ(-91.251f);

            homeMenu.SetActive(false);
            messageMenu.SetActive(false);

            MenuStateMachine = new();
            HomeState = new(this);
            MenuStateMachine.SwitchState(HomeState);

            MeshRenderer[] rendererArray = transform.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer meshRenderer in rendererArray)
            {
                Material[] uberMaterials = [.. meshRenderer.materials.Select(material => material.CreateUberShaderVariant())];
                meshRenderer.materials = uberMaterials;
            }

            screenRenderer.material = screenMaterial = new Material(screenRenderer.material);
            rimRenderer.material = screenRimMaterial = new Material(rimRenderer.material);

            Rig.OnColorChanged += SetColour;
            Events.OnRigSetInvisibleToLocal += SetVisibilityCheck;

            ConfigureWatchShared();
        }

        public void ConfigureWatchLocal()
        {
            Logging.Message($"ConfigureWatchLocal: {transform.GetPath().TrimStart('/')}");
            LocalWatch = this;

            InLeftHand = Convert.ToBoolean(Configuration.Orientation.Value);
            Configuration.Orientation.SettingChanged += (_, _) => SetHand(Convert.ToBoolean(Configuration.Orientation.Value));

            CustomPushButton homeNavigationButton = homeMenu.transform.Find("MenuSelection/Options/Home").AddComponent<CustomPushButton>();
            homeNavigationButton.OnButtonPush += _ => SetTab(WatchTab.Standard);

            mediaNavigationButton = homeMenu.transform.Find("MenuSelection/Options/Music").AddComponent<CustomPushButton>();
            mediaNavigationButton.OnButtonPush += _ => SetTab(WatchTab.MediaPlayer);
            mediaNavigationButton.Active = hasMediaSession;

            MediaManager.Instance.OnSessionFocussed += OnSessionFocussed;
            MediaManager.Instance.OnMediaChanged += OnMediaChanged;
            MediaManager.Instance.OnTimelineChanged += OnTimelineChanged;
        }

        public void ConfigureWatchShared()
        {
            Logging.Message($"ConfigureWatchShared: {transform.GetPath().TrimStart('/')}");

            SetHand(InLeftHand);
            SetTab(WatchTab.Standard);
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

        public void SetHand(bool inLeftHand)
        {
            InLeftHand = inLeftHand;

            transform.SetParent(InLeftHand ? Rig.leftHandTransform.parent : Rig.rightHandTransform.parent, false);
            transform.localPosition = InLeftHand ? Vector3.zero : new Vector3(0.01068962f, 0.040359f, -0.0006625927f);
            transform.localEulerAngles = InLeftHand ? Vector3.zero : new Vector3(-1.752f, 0.464f, 150.324f);
            transform.localScale = Vector3.one;

            if (IsLocalWatch) NetworkManager.Instance.SetProperty(Constants.NetworkProperty_Orientation, inLeftHand);
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
            float V = 0.13f * Mathf.Max((S + 1) * 0.9f, 1f);
            Color screenColour = Color.HSVToRGB(H, S, V);
            screenMaterial.color = screenColour;
        }

        private void SetTab(WatchTab newTab)
        {
            if (currentTab != newTab)
            {
                currentTab = newTab;
                menuAnimator.SetInteger(Constants.AnimatorProperty_Tab, (int)newTab);
            }
        }

        private enum WatchTab
        {
            None = -1,
            Standard,
            MediaPlayer
        }

        #endregion

        #region Media Controller

        private void OnSessionFocussed(MediaManager.Session focussedSession)
        {
            if (!IsLocalWatch) return;

            if (focussedSession != null)
            {
                OnMediaChanged(focussedSession);
                OnTimelineChanged(focussedSession);
                return;
            }

            if (hasMediaSession)
            {
                hasMediaSession = false;
                mediaNavigationButton.Active = false;

                if (currentTab == WatchTab.MediaPlayer) SetTab(WatchTab.Standard);
            }
        }

        private void OnMediaChanged(MediaManager.Session session)
        {
            if (!IsLocalWatch || MediaManager.Instance.FocussedSession != session.Id) return;

            trackTitle.text = session.Title;
            trackAuthor.text = session.Artist;
            trackThumbnail.sprite = session.Thumbnail;

            bool isValidSession = session.Title != null && session.Title.Length > 0;

            if (isValidSession != hasMediaSession)
            {
                hasMediaSession = isValidSession;
                mediaNavigationButton.Active = isValidSession;

                if (hasMediaSession) SetTab(WatchTab.MediaPlayer);
                else if (currentTab == WatchTab.MediaPlayer) SetTab(WatchTab.Standard);
            }

            if (hasMediaSession)
            {
                NetworkManager.Instance
                    .SetProperty(Constants.NetworkProperty_MediaTitle, session.Title)
                    .SetProperty(Constants.NetworkProperty_MediaArtist, session.Artist)
                    .SetProperty(Constants.NetworkProperty_MediaLength, session.EndTime);
            }
            else
            {
                NetworkManager.Instance
                    .RemoveProperty(Constants.NetworkProperty_MediaTitle)
                    .RemoveProperty(Constants.NetworkProperty_MediaArtist)
                    .RemoveProperty(Constants.NetworkProperty_MediaLength);
            }
        }

        private void OnTimelineChanged(MediaManager.Session session)
        {
            if (!IsLocalWatch || MediaManager.Instance.FocussedSession != session.Id) return;

            trackElapsed.text = TimeSpan.FromSeconds(session.Position).ToString(@"mm\:ss");
            trackRemaining.text = string.Concat("-", TimeSpan.FromSeconds(session.EndTime - session.Position).ToString(@"mm\:ss"));
            trackProgression.value = (session.EndTime > 0) ? Mathf.Lerp(trackProgression.minValue, trackProgression.maxValue, Convert.ToSingle(Math.Round(session.Position / session.EndTime, 3))) : trackProgression.minValue;
        }

        #endregion
#endif
    }
}
