using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using System.Globalization;
using UnityEngine;
using GorillaInfoWatch.Attributes;
using System;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Details")]
    public class DetailsPage : Page
    {
        private int _detailsMode = 0;

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
                    string _normalizedString = _localName.ToNormalizedName();
                    Color _localColour = GorillaTagger.Instance.offlineVRRig.playerColor;

                    AddLine(string.Concat("Name: ", _normalizedString != _localName ? string.Format("{0} ({1})", _normalizedString.ToUpper(), _localName) : _normalizedString.ToUpper()));
                    AddLine(string.Format("Colour: ({0}, {1}, {2})", Mathf.FloorToInt(_localColour.r * 9f), Mathf.FloorToInt(_localColour.g * 9f), Mathf.FloorToInt(_localColour.b * 9f)));
                    AddLine(string.Format("Turning: {0} ({1})", _textInfo.ToTitleCase(GorillaComputer.instance.GetField<string>("turnType").ToLower()), GorillaComputer.instance.GetField<int>("turnValue")));
                    AddLine(string.Concat("Microphone: ", _textInfo.ToTitleCase(GorillaComputer.instance.pttType.ToLower())));
                    AddLine(string.Concat("Queue: ", _textInfo.ToTitleCase(GorillaComputer.instance.currentQueue.ToLower())));
                    AddLine(string.Concat("Gamemode: ", _textInfo.ToTitleCase(GorillaComputer.instance.currentGameMode.Value.ToLower())));

                    break;
                case 1:

                    AddLine(string.Concat("Playtime: ", TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToString(@"h\:mm\:ss")));
                    AddLine(string.Concat("Tags: ", DataManager.GetItem("Tags", 0)));

                    break;
                case 2:

                    AddLine(string.Concat("Region: ", PhotonNetwork.CloudRegion != null ? PhotonNetwork.CloudRegion.Replace("/*", "").ToUpper() switch
                    {
                        "US" => "United States (Eastern)",
                        "USW" => "United States (Western)",
                        "EU" => "Europe",
                        _ => "Unknown"
                    } : "N/A"));
                    AddLine(string.Concat("Ping: ", (PhotonNetwork.NetworkingClient == null || !PhotonNetwork.IsConnected || PhotonNetwork.Server == ServerConnection.NameServer) ? "N/A" : PhotonNetwork.GetPing()));
                    AddLine(string.Concat("Player Count: ", NetworkSystem.Instance.GlobalPlayerCount()));
                    //AddLine(string.Concat("Player Bans: ", GorillaComputer.instance.GetField<string>("usersBanned")));

                    break;
                case 3:

                    //AddLine(string.Concat("Player-ID: ", PlayFabAuthenticator.instance._playFabPlayerIdCache));
                    AddLine(string.Concat("Version: ", GorillaComputer.instance.version));
                    AddLine(string.Concat("Platform: ", PlayFabAuthenticator.instance.GetField<string>("platform")));
                    AddLine(string.Concat("Build Date: ", GorillaComputer.instance.buildDate));

                    break;
            }
        }

        public void OnSliderChanged(object sender, SliderArgs args)
        {
            _detailsMode = args.currentValue;

            DrawPage();
            SetLines(); // just text
        }
    }
}
