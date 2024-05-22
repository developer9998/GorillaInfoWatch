using GorillaInfoWatch.Extensions;
using GorillaNetworking;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class PhysicalLine : MonoBehaviour
    {
        public NetPlayer Player;

        public TextMeshProUGUI Text;
        public Button Button;
        public Slider Slider;
        public GameObject SpeakIcon, MuteIcon;

        private GorillaPlayerScoreboardLine _scoreboardLine;

        public string PlayerText
        {
            get
            {
                if (Player == null || !Player.InRoom) return string.Empty;

                if (_scoreboardLine == null || !_scoreboardLine.gameObject.activeInHierarchy || _scoreboardLine.linePlayer.ID != Player.ID)
                    _scoreboardLine = GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault(line => line.linePlayer.ID == Player.ID && line.gameObject.activeInHierarchy);

                Color playerNameColour = Color.white;

                if (FriendLib.FriendCompatible)
                {
                    if (FriendLib.IsInFriendList(Player.UserId)) playerNameColour = FriendLib.FriendColour;
                    else if (FriendLib.IsVerified(Player.UserId)) playerNameColour = FriendLib.VerifiedColour;
                    else if (!Player.IsLocal && !FriendLib.NeedToCheckRecently(Player.UserId) && FriendLib.HasPlayedWithUsRecently(Player.UserId) == 1) playerNameColour = FriendLib.RecentlyPlayedColour;
                }

                string playerNameHtml = ColorUtility.ToHtmlStringRGB(playerNameColour);

                return string.Format("[<color=#{0}>██</color>] <color=#{1}>{2}</color>", ColorUtility.ToHtmlStringRGB(PlayerSwatchColour), playerNameHtml, (PlayFabAuthenticator.instance.GetSafety() ? Player.DefaultName : Player.NickName).ToNormalizedName().ToUpper());
            }
        }

        public Color PlayerSwatchColour
        {
            get
            {
                if (Player == null || !Player.InRoom) return Color.white;

                if (_scoreboardLine == null || _scoreboardLine.linePlayer.ID != Player.ID)
                {
                    _scoreboardLine = GorillaScoreboardTotalUpdater.allScoreboardLines.First(line => line.linePlayer.ID == Player.ID);
                }

                return _scoreboardLine.playerVRRig.setMatIndex switch
                {
                    0 => _scoreboardLine.playerVRRig.playerColor,
                    1 => new Color32(99, 2, 0, 255),
                    2 => new Color32(125, 35, 0, 255),
                    3 => new Color32(8, 45, 72, 255),
                    _ => _scoreboardLine.playerVRRig.materialsToChangeTo[_scoreboardLine.playerVRRig.setMatIndex].color
                };
            }
        }

        public void InvokeUpdate()
        {
            if (Player == null || !Player.InRoom)
            {
                Player = null;
                return;
            }

            if (_scoreboardLine)
            {
                bool isSpeaking = _scoreboardLine.speakerIcon.enabled;
                bool isMuted = _scoreboardLine.muteButton.isOn || _scoreboardLine.muteButton.isAutoOn;

                Text.text = PlayerText;

                if (SpeakIcon.activeSelf != (isSpeaking && !isMuted)) SpeakIcon.SetActive(isSpeaking && !isMuted);
                if (MuteIcon.activeSelf != isMuted) MuteIcon.SetActive(isMuted);
            }
        }
    }
}
