using GorillaInfoWatch.Behaviours;
using GorillaLocomotion;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaInfoWatch.Utilities
{
    public class TaskUtils
    {
        public static async Task YieldWebRequest(UnityWebRequest webRequest)
        {
            var completionSource = new TaskCompletionSource<UnityWebRequest>();
            ((MonoBehaviour)(Main.HasInstance ? Main.Instance : GTPlayer.Instance)).StartCoroutine(AwaitWebRequestCoroutine(webRequest, completionSource));
            await completionSource.Task;
        }

        public static async Task YieldInstruction(YieldInstruction instruction)
        {
            var completionSource = new TaskCompletionSource<YieldInstruction>();
            ((MonoBehaviour)(Main.HasInstance ? Main.Instance : GTPlayer.Instance)).StartCoroutine(AwaitInstructionCorouutine(instruction, completionSource));
            await completionSource.Task;
        }

        private static IEnumerator AwaitWebRequestCoroutine(UnityWebRequest webRequest, TaskCompletionSource<UnityWebRequest> completionSource)
        {
            yield return webRequest.SendWebRequest();
            completionSource.SetResult(webRequest);
        }

        private static IEnumerator AwaitInstructionCorouutine(YieldInstruction instruction, TaskCompletionSource<YieldInstruction> completionSource)
        {
            yield return instruction;
            completionSource.SetResult(instruction);
        }
    }
}