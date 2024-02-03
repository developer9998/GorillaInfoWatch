using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using Photon.Pun;
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
            Events.RigAdded += (PlayerArgs args) =>
            {
                foreach (IPlayerFunction function in PlayerFunctions)
                {
                    try
                    {
                        function.OnPlayerJoin?.Invoke(args);
                    }
                    catch { }
                }
            };

            Events.RigRemoved += (PlayerArgs args) =>
            {
                foreach (IPlayerFunction function in PlayerFunctions)
                {
                    try
                    {
                        function.OnPlayerLeave?.Invoke(args);
                    }
                    catch { }
                }
            };
        }
    }
}
