using GorillaInfoWatch.Models.Shortcuts;
using GorillaNetworking;
using System.Threading.Tasks;

namespace GorillaInfoWatch.Shortcuts.Rooms;

internal class Rejoin : Shortcut
{
    public override string Name => "Rejoin";
    public override string Description => "The current room containing the player is re-joined (left and then joined)";

    public override async void Invoke(bool isStateEnabled)
    {
        if (!NetworkSystem.Instance.InRoom) return;

        string roomName = NetworkSystem.Instance.RoomName;

        await NetworkSystem.Instance.ReturnToSinglePlayer();
        await Task.Delay(250);
        await PhotonNetworkController.Instance.AttemptToJoinSpecificRoomAsync(roomName, JoinType.Solo, null);
    }
}
