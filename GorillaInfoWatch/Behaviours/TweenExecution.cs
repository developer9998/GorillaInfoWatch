using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class TweenExecution : MonoBehaviour
    {
        private long ID = -1;

        private readonly Dictionary<long, TweenInfo> Tweens = new();
        private readonly Dictionary<TweenInfo, float> Durations = new();

        private float DT => Time.unscaledDeltaTime;

        public void Update()
        {
            if (Tweens.Count == 0) return;

            List<TweenInfo> completedTweens = new();

            foreach (KeyValuePair<long, TweenInfo> tweenData in Tweens)
            {
                float tweenID = tweenData.Key;
                TweenInfo tweenInfo = tweenData.Value;

                float duration = Durations[tweenInfo];
                float value = Mathf.Clamp(Mathf.Lerp(tweenInfo.End, tweenInfo.Start, duration / tweenInfo.Time), tweenInfo.End > tweenInfo.Start ? tweenInfo.Start : tweenInfo.End, tweenInfo.End > tweenInfo.Start ? tweenInfo.End : tweenInfo.Start);
                value = AnimationCurves.GetCurveForEase(tweenInfo.EaseType).Evaluate(value);

                if (duration <= 0)
                {
                    tweenInfo.OnCompleted?.Invoke(tweenInfo.End);

                    completedTweens.Add(tweenInfo);
                }
                else
                {
                    tweenInfo.Status = TweenStatus.Playing;
                    tweenInfo.OnUpdated?.Invoke(value);

                    Durations[tweenInfo] -= DT;
                }
            }

            if (completedTweens.Count == 0) return;
            completedTweens.Do(tweenInfo => CancelTween(tweenInfo));
        }

        /// <summary>
        /// Creates a tween which will be modified by the component
        /// </summary>
        /// <param name="start">The beginning value of the tween</param>
        /// <param name="end">The ending value of the tween</param>
        /// <param name="time">The duration of the tween</param>
        /// <param name="easeType">The easing type of the tween</param>
        /// <returns></returns>
        public TweenInfo ApplyTween(float start, float end, float time, AnimationCurves.EaseType easeType = AnimationCurves.EaseType.Linear)
        {
            ID++;

            TweenInfo tweenInfo = new(ID, start, end, time, easeType);
            Tweens.Add(ID, tweenInfo);
            Durations.Add(tweenInfo, time);

            return tweenInfo;
        }

        public void CancelTween(TweenInfo info)
        {
            if (info.Status == TweenStatus.Completed) return;
            info.Status = TweenStatus.Completed;

            long ID = info.ID;
            if (Tweens.ContainsKey(ID))
            {
                Durations.Remove(info);
                Tweens.Remove(ID);
            }
            else
            {
                Logging.Warning(string.Concat("Tween '", ID, "' could not be found during cancellation"));
            }
        }
    }
}
