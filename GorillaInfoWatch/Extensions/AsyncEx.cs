using GorillaInfoWatch.Behaviours;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaInfoWatch.Extensions
{
    public static class AsyncEx
    {
        private static MonoBehaviour MonoBehaviour => Main.Instance;

        public static async Task YieldAsync(this YieldInstruction instruction)
        {
            var completionSource = new TaskCompletionSource<YieldInstruction>();
            MonoBehaviour.StartCoroutine(YieldRoutine(instruction, completionSource));
            await completionSource.Task;
        }

        private static IEnumerator YieldRoutine(YieldInstruction instruction, TaskCompletionSource<YieldInstruction> completionSource)
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