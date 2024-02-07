using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using System;
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
            Events.RigAdded += (PlayerInfo args) =>
            {
                foreach (IPlayerFunction function in PlayerFunctions)
                {
                    try
                    {
                        function.OnPlayerJoin?.Invoke(args);
                    }
                    catch (Exception exception)
                    {
                        Logging.Error(string.Concat(function.GetType().Name, ".OnPlayerJoin threw an exception: ", exception.ToString()));
                    }
                }
            };

            Events.RigRemoved += (PlayerInfo args) =>
            {
                foreach (IPlayerFunction function in PlayerFunctions)
                {
                    try
                    {
                        function.OnPlayerLeave?.Invoke(args);
                    }
                    catch (Exception exception)
                    {
                        Logging.Error(string.Concat(function.GetType().Name, ".OnPlayerLeave threw an exception: ", exception.ToString()));
                    }
                }
            };
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            PlayerInfo info = new(PhotonNetwork.LocalPlayer, GorillaTagger.Instance.offlineVRRig);
            new Events().TriggerRigRemoved(info);
        }
    }
}
