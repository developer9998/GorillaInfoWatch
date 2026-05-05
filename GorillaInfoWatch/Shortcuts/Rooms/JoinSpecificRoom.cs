using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Models.Shortcuts;
using GorillaInfoWatch.Models.UserInput;
using GorillaNetworking;
using HarmonyLib;

namespace GorillaInfoWatch.Shortcuts.Rooms;

internal class JoinSpecificRoom : Shortcut
{
    public override string Name => "Join Specific Room";

    public override string Description => "Joins a specific room of the name provided by the player";

    public override void Invoke(bool isStateEnabled)
    {
        UserInput.Activate(GorillaComputer.instance.roomToJoin, UserInputBoard.Standard, 10, (sender, args) =>
        {
            string roomCode = args.Input;
            GorillaComputer.instance.roomToJoin = roomCode;

            if (!args.IsTyping)
            {
                /*
                if (GorillaComputer.instance.currentStateIndex != 0)
                {
                    GorillaComputer.instance.currentStateIndex = 0;
                    GorillaComputer.instance.SwitchState(GorillaComputer.instance.GetState(GorillaComputer.instance.currentStateIndex), true);
                }
                */

                AccessTools.Method(typeof(GorillaComputer), "ProcessRoomState").Invoke(GorillaComputer.instance, [GorillaKeyboardBindings.enter]);
            }
        });
    }
}
