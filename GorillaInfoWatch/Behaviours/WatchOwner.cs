using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    [RequireComponent(typeof(RigContainer))]
    public class WatchOwner : MonoBehaviour
    {
        private RigContainer rig_container;
        private Player creator;

        private GameObject watch_object;
        private Watch watch;

        public void Awake()
        {
            rig_container = GetComponent<RigContainer>();

            if (creator == null && Utils.InRoom(rig_container.Creator))
            {
                creator = (rig_container.Creator is PunNetPlayer pun_net_player) ? pun_net_player.PlayerRef : PhotonNetwork.CurrentRoom.GetPlayer(rig_container.Creator.ActorNumber);
            }
        }

        public async void LoadWatch()
        {
            watch_object = Instantiate(await AssetLoader.LoadAsset<GameObject>("Watch25"));
            watch = watch_object.AddComponent<Watch>();
            watch.Rig = rig_container.Rig;
            //watch_component.TimeOffset = (float)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalSeconds;
        }
    }
}