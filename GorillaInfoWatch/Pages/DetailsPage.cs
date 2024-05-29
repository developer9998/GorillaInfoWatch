using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Details")]
    public class DetailsPage : Page
    {
        private int _detailsMode = 0;

        private Gorillanalytics _analytics;

        public DetailsPage()
        {
            _analytics = UnityEngine.Object.FindObjectOfType<Gorillanalytics>();
        }

        public override void OnDisplay()
        {
            base.OnDisplay();

            SetHeader("Details", "Use scroll wheel to navigate");

            DrawPage();
            SetLines();
        }

        private void DrawPage()
        {
            ClearLines();

            string _detailsModeString = _detailsMode switch
            {
                0 => "Profile",
                1 => "Session",
                2 => "Connection",
                3 => "Build",
                _ => "TBA"
            };
            TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;

            AddLine("Type: " + _detailsModeString, slider: new LineSlider(OnSliderChanged, 0, 4, _detailsMode)).AddLine();

            switch (_detailsMode)
            {
                case 0:

                    string _localName = GorillaComputer.instance.savedName;
                    string _normalizedString = _localName.NormalizeName();
                    Color _localColour = GorillaTagger.Instance.offlineVRRig.playerColor;

                    AddLine(string.Concat("Name: ", _normalizedString != _localName ? string.Format("{0} ({1})", _normalizedString.ToUpper(), _localName) : _normalizedString.ToUpper()));
                    AddLine(string.Format("Colour: ({0}, {1}, {2})", Mathf.FloorToInt(_localColour.r * 9f), Mathf.FloorToInt(_localColour.g * 9f), Mathf.FloorToInt(_localColour.b * 9f)));
                    AddLine(string.Format("Turning: {0} ({1})", _textInfo.ToTitleCase(GorillaComputer.instance.GetField<string>("turnType").ToLower()), GorillaComputer.instance.GetField<int>("turnValue")));
                    AddLine(string.Concat("Microphone: ", _textInfo.ToTitleCase(GorillaComputer.instance.pttType.ToLower())));
                    AddLine(string.Concat("Queue: ", _textInfo.ToTitleCase(GorillaComputer.instance.currentQueue.ToLower())));

                    GetModeInfo(GorillaComputer.instance.currentGameMode.Value, out string mode, out bool isModded);
                    AddLine(string.Concat("Gamemode: ", mode == "unknown"
                        ? _textInfo.ToTitleCase(GorillaComputer.instance.currentGameMode.Value.ToLower())
                        : (isModded
                            ? string.Concat("Modded ", _textInfo.ToTitleCase(mode.ToLower()))
                            : _textInfo.ToTitleCase(mode.ToLower())
                          )));

                    break;
                case 1:

                    AddLine(string.Concat("Playtime: ", TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToString(@"h\:mm\:ss")));
                    AddLine(string.Concat("Tags: ", Metadata.GetItem("Tags", 0)));

                    break;
                case 2:

                    AddLine(string.Concat("Player Count: ", NetworkSystem.Instance.GlobalPlayerCount())).AddLine();

                    bool validServerConnection = PhotonNetwork.NetworkingClient != null && PhotonNetwork.IsConnected && PhotonNetwork.Server != ServerConnection.NameServer;
                    if (validServerConnection)
                    {
                        AddLine(string.Concat("Region: ", PhotonNetwork.CloudRegion.Replace("/*", "").ToUpper() switch
                        {
                            "US" => "USA, East",
                            "USW" => "USA, West",
                            "EU" => "Europe",
                            _ => "Unknown"
                        }));
                        AddLine(string.Concat("Ping: ", PhotonNetwork.GetPing()));

                        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out object obj) && obj != null)
                        {
                            GetModeInfo(obj.ToString(), out string serverMode, out bool serverModded);
                            GetQueueInfo(obj.ToString(), out string serverQueue);

                            AddLine().AddLine(string.Concat("Gamemode: ", serverMode == "unknown"
                            ? GorillaGameManager.instance != null ? GorillaGameManager.instance.GameModeName() : "Unknown"
                            : (serverModded
                                ? string.Concat("Modded ", _textInfo.ToTitleCase(serverMode.ToLower()))
                                : _textInfo.ToTitleCase(serverMode.ToLower())
                              )));
                            AddLine(string.Concat("Queue: ", _textInfo.ToTitleCase(serverQueue.ToLower())));
                        }
                    }

                    //AddLine(string.Concat("Player Bans: ", GorillaComputer.instance.GetField<string>("usersBanned")));

                    break;
                case 3:

                    //AddLine(string.Concat("Player-ID: ", PlayFabAuthenticator.instance._playFabPlayerIdCache));
                    AddLine(string.Concat("Platform: ", PlayFabAuthenticator.instance.GetField<string>("platform")));
                    AddLine(string.Concat("Version: ", GorillaComputer.instance.version));
                    AddLine(string.Concat("Build Date: ", GorillaComputer.instance.buildDate));

                    break;
            }
        }

        private void GetModeInfo(string reference, out string mode, out bool modded)
        {
            List<string> modes = _analytics.modes;
            mode = modes.Find(reference.Contains) ?? "unknown";
            modded = reference.Contains(Utilla.Models.Gamemode.GamemodePrefix);
        }

        private void GetQueueInfo(string reference, out string queue)
        {
            List<string> queues = _analytics.queues;
            queue = queues.Find(reference.Contains) ?? "unknown";
        }

        public void OnSliderChanged(object sender, SliderArgs args)
        {
            _detailsMode = args.currentValue;

            DrawPage();
            SetLines(); // just text
        }
    }
}
