using GorillaInfoWatch.Interfaces;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using Zenject;

namespace GorillaInfoWatch.Behaviours
{
    public class PlayerExecution : MonoBehaviourPunCallbacks, IInitializable
    {
        private static List<IPlayerFunction> PlayerFunctions = new();

        [Inject]
        public void Construct(List<IPlayerFunction> functions)
        {
            PlayerFunctions = functions;
        }

        public void Initialize()
        {
            Events.RigAdded += (Player Player, VRRig Rig) =>
            {
                foreach (IPlayerFunction function in PlayerFunctions)
                {
                    try
                    {
                        function.OnPlayerJoin?.Invoke(Player, Rig);
                    }
                    catch { }
                }
            };

            Events.RigRemoved += (Player Player, VRRig Rig) =>
            {
                foreach (IPlayerFunction function in PlayerFunctions)
                {
                    try
                    {
                        function.OnPlayerLeave?.Invoke(Player, Rig);
                    }
                    catch { }
                }
            };
        }
    }
}
