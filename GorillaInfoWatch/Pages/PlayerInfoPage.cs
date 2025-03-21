using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Pages
{
    public class PlayerInfoPage : Models.WatchScreen
    {
        public override string Title => "Details";

        public override string Description => "n/a";
        
        /*
        private NetPlayer player;
        private string _normalizedString;
        private GorillaPlayerScoreboardLine scoreboard_line;

        private bool _usingReportOptions;
        GorillaPlayerLineButton.ButtonType _reportType;

        public override void OnDisplay()
        {
            base.OnDisplay();

            player = (NetPlayer)Parameters[0];
            scoreboard_line = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.UserId == player?.UserId);

            if ((scoreboard_line == null && NetworkSystem.Instance.InRoom) || !NetworkSystem.Instance.InRoom)
            {
                
            }

            _normalizedString = (PlayFabAuthenticator.instance.GetSafety() ? player.DefaultName : player.NickName).NormalizeName();
            scoreboard_line = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.ActorNumber == player.ActorNumber);

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

            scoreboard_line = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.ActorNumber == player.ActorNumber);
            string name = PlayFabAuthenticator.instance.GetSafety() ? player.DefaultName : player.NickName;

            AddPlayer(player);
            AddLine(string.Concat("Name: ", _normalizedString != name ? string.Format("{0} ({1})", _normalizedString.ToUpper(), name) : _normalizedString.ToUpper()));
            AddLine(string.Concat("Master: ", player.IsMasterClient ? "True" : "False"));
            AddLine(string.Concat("Voice: ", scoreboard_line.playerVRRig.GetComponent<GorillaSpeakerLoudness>().IsMicEnabled ? (scoreboard_line.playerVRRig.localUseReplacementVoice || scoreboard_line.playerVRRig.remoteUseReplacementVoice ? "Monke" : "Human") : "None"));

            if (!player.IsLocal)
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
                    AddLine(string.Concat(scoreboard_line.muteButton.isOn ? "Muted" : "Mute"), new LineButton(OnButtonSelected, 0, true, scoreboard_line.muteButton.isOn));
                    if (FriendLib.FriendCompatible) AddLine(string.Concat(FriendLib.IsInFriendList(player.UserId) ? "Friend!" : "Add Friend"), new LineButton(OnButtonSelected, 1, true, FriendLib.IsInFriendList(player.UserId)));
                    AddLine().AddLine(scoreboard_line.reportButton.isOn ? "<color=red>Reported</color>" : "Report", new LineButton(OnButtonSelected, 2, true, scoreboard_line.reportButton.isOn));
                }
            }
        }

        public void OnSliderChanged(object sender, SliderArgs sliderArgs)
        {
            _reportType = sliderArgs.currentValue switch
            {
                0 => GorillaPlayerLineButton.ButtonType.Toxicity,
                1 => GorillaPlayerLineButton.ButtonType.HateSpeech,
                2 => GorillaPlayerLineButton.ButtonType.Cheating,
                _ => throw new System.IndexOutOfRangeException()
            };

            DrawLines();
            UpdateLines();
        }

        public void OnButtonSelected(object sender, ButtonArgs args)
        {
            scoreboard_line = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.ActorNumber == player.ActorNumber);

            if (args.returnIndex == 1) // gorillafriends does all this for me !!! <333
            {
                if (FriendLib.IsInFriendList(player.UserId))
                {
                    FriendLib.RemoveFriend(player);
                }
                else
                {
                    FriendLib.AddFriend(player);
                }

                DrawLines();
                UpdateLines();

                return;
            }

            List<GorillaPlayerScoreboardLine> lines = [.. GorillaScoreboardTotalUpdater.allScoreboardLines.Where(line => line.linePlayer.ActorNumber == player.ActorNumber)];
            switch (args.returnIndex)
            {
                case 0:

                    bool isMuted = PlayerPrefs.GetInt(player.UserId, 0) != 0;

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
        */
    }
}
