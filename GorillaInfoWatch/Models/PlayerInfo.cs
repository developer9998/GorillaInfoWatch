using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;

namespace GorillaInfoWatch.Models
{
    public class PlayerInfo
    {
        public Player Player { get; }

        public VRRig Rig { get; }
        public PhotonView View { get; }
        public PhotonVoiceView VoiceView { get; }

        public PlayerInfo(Player player, VRRig rig)
        {
            Player = player;
            Rig = rig;
        }

        public PlayerInfo(Player player, VRRig rig, PhotonView view, PhotonVoiceView voiceView) : this(player, rig)
        {
            View = view;
            VoiceView = voiceView;
        }
    }
}
