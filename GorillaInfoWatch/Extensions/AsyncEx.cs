using GorillaInfoWatch.Behaviours;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaInfoWatch.Extensions
{
    public static class AsyncEx
    {
        private static MonoBehaviour MonoBehaviour => Main.Instance;

        public static async Task AsAwaitable(this YieldInstruction instruction)
        {
            var completionSource = new TaskCompletionSource<YieldInstruction>();
            MonoBehaviour.StartCoroutine(AwaitableRoutine(instruction, completionSource));
            await completionSource.Task;
        }

        private static IEnumerator AwaitableRoutine(YieldInstruction instruction, TaskCompletionSource<YieldInstruction> completionSource)
        {
            yield return instruction;
            completionSource.SetResult(instruction);
            yield break;
        }

        /*
        public static async Task YieldAsync(UnityWebRequest webRequest)
        {

        }
        */
    }
}