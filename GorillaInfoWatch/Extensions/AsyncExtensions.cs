using System.Collections;
using System.Threading.Tasks;
using GorillaInfoWatch.Behaviours;
using UnityEngine;

namespace GorillaInfoWatch.Extensions;

internal static class AsyncExtensions
{
    private static MonoBehaviour MonoBehaviour => Main.Instance;

    public static async Task AsAwaitable(this YieldInstruction instruction)
    {
        TaskCompletionSource<YieldInstruction> completionSource = new();
        IEnumerator                            coroutine        = AwaitableRoutine(instruction, completionSource);
        MonoBehaviour.StartCoroutine(coroutine);
        await completionSource.Task;
        MonoBehaviour.StopCoroutine(coroutine);
    }

    public static async Task AsAwaitable(this CustomYieldInstruction instruction)
    {
        TaskCompletionSource<CustomYieldInstruction> completionSource = new();
        IEnumerator                                  coroutine        = AwaitableRoutine(instruction, completionSource);
        MonoBehaviour.StartCoroutine(coroutine);
        await completionSource.Task;
        MonoBehaviour.StopCoroutine(coroutine);
    }

    private static IEnumerator AwaitableRoutine(YieldInstruction                       instruction,
                                                TaskCompletionSource<YieldInstruction> completionSource)
    {
        yield return instruction;
        completionSource.SetResult(instruction);
    }

    private static IEnumerator AwaitableRoutine(CustomYieldInstruction                       instruction,
                                                TaskCompletionSource<CustomYieldInstruction> completionSource)
    {
        yield return instruction;
        completionSource.SetResult(instruction);
    }
}