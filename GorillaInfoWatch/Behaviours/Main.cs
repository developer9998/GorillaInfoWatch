using BepInEx.Bootstrap;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Pages;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GorillaInfoWatch.Behaviours
{
    public class Main : MonoBehaviourPunCallbacks, IInitializable
    {
        private bool _initialized;

        private Relations _relations;

        // Tools
        private Configuration _config;

        // Pages
        private HomePage _homePage;
        private ModRoomWarningPage _modWarnPage;
        private List<Type> _allPages;
        private PageManager _pageManager;
        private WindowPlaceholderFactory _pageFactory;
        private readonly Dictionary<Type, IPage> _pageCache = [];

        // Assets
        private GameObject _customWatch, _customMenu;
        private AudioClip _clickAudio;

        // Display
        private int _sceneIndex;
        private MenuConstructor _menuConstructor;
        private TextMeshProUGUI _currentPageLabel;
        private Button _backPGButton, _nextPGButton, _returnButton, _reloadButton;

        [Inject]
        public async void Construct(AssetLoader assetLoader, WindowPlaceholderFactory pageFactory, HomePage homeWindow, ModRoomWarningPage modWarnPage, Configuration configuration)
        {
            if (_initialized) return;

            _initialized = true;
            _relations = new Relations()
            {
                Main = this,
                AssetLoader = assetLoader,
                Config = configuration
            };

            _config = configuration;

            _pageFactory = pageFactory;

            _homePage = homeWindow;
            _modWarnPage = modWarnPage;

            _pageCache.Add(typeof(HomePage), _homePage);
            _pageCache.Add(typeof(ModRoomWarningPage), _modWarnPage);

            _pageManager = new PageManager
            {
                ModWarnPage = _modWarnPage
            };

            _pageManager.OnHeaderSet += SetHeader;
            _pageManager.OnLinesSet += SetLines;
            _pageManager.OnLinesUpdated += UpdateLines;
            _pageManager.OnPageSwitched += SetPage;

            _customWatch = Instantiate(await assetLoader.LoadAsset<GameObject>("Watch"));

            _customWatch.transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent, false);

            _customWatch.transform.localPosition = Vector3.zero;
            _customWatch.transform.localEulerAngles = Vector3.zero;
            _customWatch.transform.localScale = Vector3.one;

            _customWatch.AddComponent<WatchModel>().Relations = _relations;

            _customMenu = Instantiate(await assetLoader.LoadAsset<GameObject>("Menu"));

            _customMenu.transform.SetParent(_customWatch.transform.Find("Point"));

            _customMenu.transform.localPosition = Vector3.zero;
            _customMenu.transform.localEulerAngles = Vector3.zero;

            _customMenu.AddComponent<Panel>();

            _clickAudio = await assetLoader.LoadAsset<AudioClip>("Click");

            _menuConstructor = new MenuConstructor()
            {
                Relations = _relations,
                Background = _customMenu.transform.Find("Canvas/Background").GetComponent<Image>(),
                Header = _customMenu.transform.Find("Canvas/Header").GetComponent<TextMeshProUGUI>(),
                LineGrid = _customMenu.transform.Find("Canvas/Lines"),
            };
            _menuConstructor.InitializeLines();

            TimeDisplay watchTime = _customWatch.AddComponent<TimeDisplay>();

            watchTime.Text = _customWatch.transform.Find("Hand Menu/Canvas/Time Display/Time").GetComponent<TMP_Text>();
            watchTime.Relations = _relations;

            WatchTrigger watchTrigger = _customWatch.transform.Find("Hand Model/Trigger").gameObject.AddComponent<WatchTrigger>();

            watchTrigger.Menu = _customMenu;
            watchTrigger.Relations = _relations;

            _currentPageLabel = _customMenu.transform.Find("Canvas/Page Text").GetComponent<TextMeshProUGUI>();

            _nextPGButton = _customMenu.transform.Find("Canvas/Button_Next").AddComponent<Button>();

            _nextPGButton.Relations = _relations;
            _nextPGButton.OnPressed = () =>
            {
                _sceneIndex++;
                _pageManager.DisplayPage();
            };

            _nextPGButton.gameObject.SetActive(false);

            _backPGButton = _customMenu.transform.Find("Canvas/Button_Previous").AddComponent<Button>();

            _backPGButton.Relations = _relations;
            _backPGButton.OnPressed = () =>
            {
                _sceneIndex--;
                _pageManager.DisplayPage();
            };

            _backPGButton.gameObject.SetActive(false);

            _returnButton = _customMenu.transform.Find("Canvas/Button_Return").AddComponent<Button>();

            _returnButton.Relations = _relations;
            _returnButton.OnPressed = () =>
            {
                if (_pageManager.CurrentPage.CallerPage != null)
                {
                    SetPage(_pageManager.CurrentPage.GetType(), _pageManager.CurrentPage.CallerPage, _pageCache[_pageManager.CurrentPage.CallerPage].Parameters, true);
                }
            };

            _returnButton.gameObject.SetActive(false);

            _reloadButton = _customMenu.transform.Find("Canvas/Button_Redraw").AddComponent<Button>();

            _reloadButton.Relations = _relations;
            _reloadButton.OnPressed = () =>
            {
                _pageManager.DisplayPage();
            };

            NetworkSystem.Instance.OnReturnedToSinglePlayer += InitiateRefresh;
            NetworkSystem.Instance.OnMultiplayerStarted += InitiateRefresh;
            NetworkSystem.Instance.OnPlayerJoined += InitiateRefresh;
            NetworkSystem.Instance.OnPlayerLeft += InitiateRefresh;

            Dictionary<string, BepInEx.PluginInfo> loadedPlugins = Chainloader.PluginInfos;
            _allPages = [.. loadedPlugins.Select(plugin => plugin.Value.Instance.GetType().Assembly).SelectMany(a => a.GetTypes()).Where(type => type.IsSubclassOf(typeof(Page))).Where(type => !_pageCache.ContainsKey(type))];

            FriendLib.InitializeLib(loadedPlugins);
            _homePage.SetEntries(_allPages);
            _pageManager.SwitchPage(_homePage, []);
        }

        public void Initialize()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
        }

        public void FixedUpdate()
        {
            if (_menuConstructor != null)
            {
                foreach (PhysicalLine line in _menuConstructor.Lines)
                {
                    line.InvokeUpdate();
                }
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            try
            {
                if (FriendLib.IsFriend(newPlayer.UserId))
                {
                    GorillaTagger.Instance.StartVibration(true, 0.04f, 0.2f);
                    _customWatch.GetComponent<WatchModel>().ShowFriendPanel(newPlayer, true);
                }
            }
            catch
            {
                Logging.Error("womp womp");
            }

        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            try
            {
                if (FriendLib.IsFriend(otherPlayer.UserId))
                {
                    GorillaTagger.Instance.StartVibration(true, 0.04f, 0.2f);
                    _customWatch.GetComponent<WatchModel>().ShowFriendPanel(otherPlayer, false);
                }
            }
            catch
            {
                Logging.Error("womp womp");
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
            InitiateRefresh();
        }

        public void InitiateRefresh(int ID) => InitiateRefresh();

        public void InitiateRefresh()
        {
            _pageManager.DisplayPage();
        }

        public void PressButton(Button button, bool isLeftHand)
        {
            if (button)
            {
                AudioSource handSource = isLeftHand
                    ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                    : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

                AudioClip clip = button.TryGetComponent(out AudioSource component) ? component.clip : _clickAudio;
                handSource.PlayOneShot(clip, 0.2f);
            }
        }

        public void PressSlider(Slider slider, bool isLeftHand)
        {
            if (slider)
            {
                AudioSource handSource = isLeftHand
                    ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer
                    : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;

                AudioClip clip = _clickAudio;
                handSource.PlayOneShot(clip, 0.2f);
            }
        }

        public void SetHeader(HeaderLine header)
        {
            if (header != null)
            {
                string headerText = string.Format("<size=150%><align=\"center\">{0}</align></size>\n<align=\"center\">{1}</align>", header.heading, header.caption);
                _menuConstructor.Header.text = headerText;
            }
        }

        public void UpdateLines(object[] lines)
        {
            if (lines == null || !lines.Any()) return;

            for (int i = _sceneIndex * Constants.EntriesPerScene; i < GetDisplayedLineCount(lines.Length); i++)
            {
                int index = i - _sceneIndex * Constants.EntriesPerScene;
                PhysicalLine physicalLine = _menuConstructor.Lines.ElementAtOrDefault(index);

                if (!physicalLine || !physicalLine.gameObject.activeSelf) continue;

                if (lines[i] is PlayerLine)
                {
                    physicalLine.Text.text = physicalLine.PlayerText;
                }
                else if (lines[i] is GenericLine genLine)
                {
                    physicalLine.Text.text = genLine.Text;
                }
            }

            _returnButton.gameObject.SetActive(_pageManager.CurrentPage.CallerPage != null);
        }

        public void SetLines(object[] lines)
        {
            _menuConstructor.ClearLines();
            ValidateSceneIndex(lines.Length);

            if (lines != null)
            {
                for (int i = _sceneIndex * Constants.EntriesPerScene; i < GetDisplayedLineCount(lines.Length); i++)
                {
                    if (lines[i] is PlayerLine playerLine)
                    {
                        _menuConstructor.AddLine(playerLine);
                    }
                    else if (lines[i] is GenericLine genLine)
                    {
                        _menuConstructor.AddLine(genLine);
                    }
                }
            }

            _returnButton.gameObject.SetActive(_pageManager.CurrentPage.CallerPage != null);
        }

        public int GetDisplayedLineCount(int totalLineCount)
        {
            int EndLine = _sceneIndex * Constants.EntriesPerScene + Constants.EntriesPerScene;
            return EndLine > totalLineCount ? totalLineCount : EndLine;
        }

        public void ValidateSceneIndex(int totalLineCount)
        {
            if (_sceneIndex < 0) _sceneIndex = 0;

            _nextPGButton.gameObject.SetActive(totalLineCount / (float)Constants.EntriesPerScene > 1f && _sceneIndex != Mathf.FloorToInt(totalLineCount / (float)Constants.EntriesPerScene));
            _backPGButton.gameObject.SetActive(_sceneIndex > 0);
            _currentPageLabel.text = _nextPGButton.gameObject.activeSelf || _backPGButton.gameObject.activeSelf
                ? string.Format("{0}/{1}", _sceneIndex + 1, Mathf.FloorToInt(totalLineCount / (float)Constants.EntriesPerScene) + 1)
                : string.Empty;
        }

        public void SetPage(Type origin, Type type, object[] parameters) => SetPage(origin, type, parameters, false);

        public void SetPage(Type origin, Type type, object[] parameters, bool isReturnApplied)
        {
            parameters ??= [];
            try
            {
                IPage page = GetPage(type);

                if (type == typeof(ModRoomWarningPage) || origin == typeof(ModRoomWarningPage))
                {
                    page.CallerPage = GetPage(origin).CallerPage;
                }
                else if (!isReturnApplied && origin != page.CallerPage && GetPage(origin).CallerPage != type)
                {
                    page.CallerPage = origin;
                }

                _sceneIndex = 0;

                _returnButton.gameObject.SetActive(page.CallerPage != null);
                _pageManager.SwitchPage(page, parameters);
            }
            catch (Exception exception)
            {
                Logging.Error(string.Concat("SetPage threw an exception: ", exception.ToString()));
            }
        }

        private IPage GetPage(Type type)
        {
            if (_pageCache.TryGetValue(type, out IPage view)) return view;

            if (!_allPages.Contains(type))
            {
                Logging.Error(string.Format("Type {0} can not be used with GetPage.", nameof(type)));
                return null;
            }

            IPage newPage = null;
            try
            {
                newPage = _pageFactory.Create(type);
                _pageCache.Add(type, newPage);

                Logging.Info(string.Concat("Created new page of type ", type));
            }
            catch (Exception exception)
            {
                Logging.Error(string.Concat("GetPage threw an exception: ", exception.ToString()));
            }

            return newPage;
        }

        private void OnEventReceived(EventData photonEvent)
        {
            if (photonEvent.Code == GorillaTagManager.ReportInfectionTagEvent) // The game doesn't use this constant, rather it's hardcoded into the event for some reason
            {
                object[] data = (object[])photonEvent.CustomData;
                Player taggingPlayer = GameMode.ParticipatingPlayers.FirstOrDefault(player => player.UserId == (string)data[0]);

                if (taggingPlayer != null && taggingPlayer.IsLocal)
                {
                    DataManager.AddItem("Tags", DataManager.GetItem("Tags", 0) + 1);
                }
            }
        }
    }
}
