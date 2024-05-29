using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Pages
{
    public class PlayerInfoPage : Page
    {
        private NetPlayer _netPlayer;
        private string _normalizedString;
        private GorillaPlayerScoreboardLine _scoreboardLine;

        private bool _usingReportOptions;
        GorillaPlayerLineButton.ButtonType _reportType;

        public override void OnDisplay()
        {
            base.OnDisplay();

            _netPlayer = (NetPlayer)Parameters[0];

            if (_netPlayer == null || !_netPlayer.InRoom || _netPlayer.IsNull || NetworkSystem.Instance.GetUserID(_netPlayer.ID) == null)
            {
                ShowPage(typeof(PlayerListPage));
                return;
            }

            _normalizedString = (PlayFabAuthenticator.instance.GetSafety() ? _netPlayer.DefaultName : _netPlayer.NickName).NormalizeName();
            _scoreboardLine = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.ID == _netPlayer.ID);

            DrawLines();
            SetLines();
        }

        public override void OnClose()
        {
            base.OnClose();

            _usingReportOptions = false;
            _reportType = GorillaPlayerLineButton.ButtonType.Toxicity;
        }

        public void DrawLines()
        {
            ClearLines();

            _scoreboardLine = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.ID == _netPlayer.ID);
            string name = PlayFabAuthenticator.instance.GetSafety() ? _netPlayer.DefaultName : _netPlayer.NickName;

            AddPlayer(_netPlayer);
            AddLine(string.Concat("Name: ", _normalizedString != name ? string.Format("{0} ({1})", _normalizedString.ToUpper(), name) : _normalizedString.ToUpper()));
            AddLine(string.Concat("Master: ", _netPlayer.IsMaster ? "True" : "False"));
            AddLine(string.Concat("Voice: ", _scoreboardLine.playerVRRig.GetComponent<GorillaSpeakerLoudness>().IsMicEnabled ? (_scoreboardLine.playerVRRig.localUseReplacementVoice || _scoreboardLine.playerVRRig.remoteUseReplacementVoice ? "Monke" : "Human") : "None"));

            if (!_netPlayer.IsLocal)
            {
                AddLine();

                if (_usingReportOptions)
                {
                    AddLine("Type: " + _reportType, slider: new LineSlider(OnSliderChanged, 0, 3));
                    AddLine("Submit", button: new LineButton(OnButtonSelected, 3));
                    AddLine("Cancel", button: new LineButton(OnButtonSelected, 4));
                }
                else
                {
                    AddLine(string.Concat(_scoreboardLine.muteButton.isOn ? "Muted" : "Mute"), new LineButton(OnButtonSelected, 0, true, _scoreboardLine.muteButton.isOn));
                    if (FriendLib.FriendCompatible) AddLine(string.Concat(FriendLib.IsInFriendList(_netPlayer.UserId) ? "Friend!" : "Add Friend"), new LineButton(OnButtonSelected, 1, true, FriendLib.IsInFriendList(_netPlayer.UserId)));
                    AddLine().AddLine(_scoreboardLine.reportButton.isOn ? "<color=red>Reported</color>" : "Report", new LineButton(OnButtonSelected, 2, true, _scoreboardLine.reportButton.isOn));
                }
            }
        }

        public void OnSliderChanged(object sender, SliderArgs sliderArgs)
        {
            _reportType = sliderArgs.currentValue switch
            {
                0 => GorillaPlayerLineButton.ButtonType.Toxicity,
                1 => GorillaPlayerLineButton.ButtonType.HateSpeech,
                _ => GorillaPlayerLineButton.ButtonType.Cheating,
            };

            DrawLines();
            UpdateLines();
        }

        public void OnButtonSelected(object sender, ButtonArgs args)
        {
            _scoreboardLine = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.ID == _netPlayer.ID);

            if (args.returnIndex == 1) // gorillafriends does all this for me !!! <333
            {
                if (FriendLib.IsInFriendList(_netPlayer.UserId))
                {
                    FriendLib.RemoveFriend(_netPlayer);
                }
                else
                {
                    FriendLib.AddFriend(_netPlayer);
                }

                DrawLines();
                UpdateLines();

                return;
            }

            List<GorillaPlayerScoreboardLine> lines = [.. GorillaScoreboardTotalUpdater.allScoreboardLines.Where(line => line.linePlayer.ID == _netPlayer.ID)];
            switch (args.returnIndex)
            {
                case 0:

                    bool isMuted = PlayerPrefs.GetInt(_netPlayer.UserId, 0) != 0;

                    lines.First().PressButton(!isMuted, GorillaPlayerLineButton.ButtonType.Mute);
                    lines.ForEach(line =>
                    {
                        line.muteButton.isOn = !isMuted;
                        line.muteButton.UpdateColor();
                    });

                    DrawLines();
                    UpdateLines();

                    break;
                case 2:

                    lines.ForEach(line => line.PressButton(false, GorillaPlayerLineButton.ButtonType.Report));
                    lines.ForEach(line => line.reportButton.gameObject.SetActive(true));

                    _usingReportOptions = true;
                    _reportType = GorillaPlayerLineButton.ButtonType.Toxicity;

                    DrawLines();
                    SetLines();

                    break;
                case 3:

                    lines.First().PressButton(false, _reportType);
                    lines.ForEach(line =>
                    {
                        line.reportButton.isOn = true;
                        line.reportButton.UpdateColor();
                        line.reportButton.gameObject.SetActive(true);
                        line.toxicityButton.SetActive(false);
                        line.hateSpeechButton.SetActive(false);
                        line.cheatingButton.SetActive(false);
                        line.cancelButton.SetActive(false);
                    });

                    _usingReportOptions = false;

                    DrawLines();
                    SetLines();

                    break;
                default:

                    lines.First().PressButton(false, GorillaPlayerLineButton.ButtonType.Cancel);
                    lines.ForEach(line =>
                    {
                        line.reportInProgress = false;
                        line.reportButton.gameObject.SetActive(true);
                        line.toxicityButton.SetActive(false);
                        line.hateSpeechButton.SetActive(false);
                        line.cheatingButton.SetActive(false);
                        line.cancelButton.SetActive(false);
                    });

                    _usingReportOptions = false;

                    DrawLines();
                    SetLines();

                    break;
            };
        }
    }
}
