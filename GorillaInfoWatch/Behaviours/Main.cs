using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tabs;
using GorillaInfoWatch.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : MonoBehaviour, IInitializable
    {
        private bool Initialized;

        private Configuration Configuration;
        private TabPlaceholderFactory TabFactory;
        private MainTab InitialTab;

        private TabManager TabManager;

        private GameObject Watch, Menu;
        private AudioClip Click;

        private MenuDisplayInfo DisplayInfo;
        private float RefreshTime;

        private readonly Dictionary<Type, ITab> TabCache = new();

        [Inject]
        public async void Construct(AssetLoader assetLoader, TabPlaceholderFactory tabFactory, MainTab initialTab, List<IEntry> entries, Configuration configuration)
        {
            if (Initialized) return;
            Initialized = true;
            
            Configuration = configuration;

            TabFactory = tabFactory;

            InitialTab = initialTab;
            TabCache.Add(typeof(MainTab), InitialTab);

            TabManager = new TabManager();
            TabManager.OnTextChanged += SetText;
            TabManager.OnNewTabRequested += SetTab;

            #region Initialize assets 

            #region Watch

            Watch = Instantiate(await assetLoader.LoadAsset<GameObject>("Watch"));

            Watch.transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent, false);
            Watch.transform.localPosition = Vector3.zero;
            Watch.transform.localEulerAngles = Vector3.zero;
            Watch.transform.localScale = Vector3.one;
            Watch.AddComponent<WatchVisibility>();

            #endregion
            #region Menu

            Menu = Instantiate(await assetLoader.LoadAsset<GameObject>("Menu"));
            Menu.transform.SetParent(Watch.transform.Find("Point"));
            Menu.transform.localPosition = Vector3.zero;
            Menu.transform.localEulerAngles = Vector3.zero;
            Menu.AddComponent<MenuBase>();

            #endregion

            Click = await assetLoader.LoadAsset<AudioClip>("Click");

            #endregion

            DisplayInfo = new MenuDisplayInfo()
            {
                Text = Menu.transform.Find("Canvas/Text (Legacy)").GetComponent<Text>(),
                Background = Menu.transform.Find("Canvas/Panel").GetComponent<Image>()
            };

            Transform buttonContainer = Menu.transform.Find("Buttons");
            for (int i = 0; i < buttonContainer.childCount; i++)
            {
                WatchButton button = buttonContainer.GetChild(i).gameObject.AddComponent<WatchButton>();
                button.Main = this;
                button.Type = (ButtonType)Enum.Parse(typeof(ButtonType), button.name);
            }

            Watch.AddComponent<TimeDisplay>().Text = Watch.transform.Find("Hand Menu/Canvas/Time").GetComponent<Text>();

            WatchTrigger watchTrigger = Watch.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();
            watchTrigger.Menu = Menu;

            TabManager.SetTab(InitialTab, null);
            InitialTab.SetEntries(entries);
        }

        public void Initialize()
        {

        }

        public void Update()
        {
            if (Configuration != null && Configuration.RefreshRate.Value != 0 && TabManager != null && TabManager.Tab != null && Time.time > (RefreshTime + (1f / Configuration.RefreshRate.Value)))
            {
                RefreshTime = Time.time;
                TabManager.Tab.OnScreenRefresh();
            }
        }

        public void PressButton(WatchButton button, bool isLeftHand)
        {
            if (!button || TabManager.Tab == null) return;
            TabManager.Tab.OnButtonPress(button.Type);

            AudioSource handSource = isLeftHand 
                ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer 
                : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
            handSource.PlayOneShot(Click, 0.8f);
        }

        public void SetText(string text)
        {
            if (DisplayInfo != null && DisplayInfo.Text)
            {
                DisplayInfo.SetText(text);
            }
        }

        public void SetTab(Type type, object[] Parameters)
        {
            try
            {
                ITab tab = GetTab(type);
                if (tab != null)
                {
                    TabManager.SetTab(tab, Parameters);
                    RefreshTime = Time.time;
                }
            }
            catch(Exception exception)
            {
                Logging.Error(string.Concat("SetTab threw an exception: ", exception.ToString()));
            }
        }

        private ITab GetTab(Type type)
        {
            if (TabCache.TryGetValue(type, out ITab view)) return view;

            ITab newTab = null;
            try
            {
                newTab = TabFactory.Create(type);
                TabCache.Add(type, newTab);

                Logging.Info(string.Concat("Created new tab of type ", nameof(type)));
            }
            catch(Exception exception)
            {
                Logging.Error(string.Concat("GetTab threw an exception: ", exception.ToString()));
            }

            return newTab;
        }
    }
}
