using GorillaInfoWatch.Models.Shortcuts;
using UnityEngine;

namespace GorillaInfoWatch.Shortcuts.Rooms;

internal class CopyRoomName : Shortcut
{
    public override string Name => "Copy Room Name";
    public override string Description => "Copies the name of the current room to your clipboard";

    public override void Invoke(bool isStateEnabled)
    {
        if (NetworkSystem.Instance.InRoom)
        {
            string roomName = NetworkSystem.Instance.RoomName;
            GUIUtility.systemCopyBuffer = roomName;
        }
    }
}
