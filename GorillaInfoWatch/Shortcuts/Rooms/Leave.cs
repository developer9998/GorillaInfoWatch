using GorillaInfoWatch.Models.Shortcuts;

namespace GorillaInfoWatch.Shortcuts.Rooms;

internal class Leave : Shortcut
{
    public override string Name => "Leave";
    public override string Description => "The current room containing the player is left";

    public override void Invoke(bool isStateEnabled)
    {
        if (!NetworkSystem.Instance.InRoom) return;
        NetworkSystem.Instance.ReturnToSinglePlayer();
    }
}
