using System;

namespace GorillaInfoWatch.Models
{
    public class TweenInfo
    {
        public TweenStatus Status;
        public readonly long ID;

        public readonly float Start, End, Time;
        public readonly AnimationCurves.EaseType EaseType;

        public Action<float> OnUpdated, OnCompleted;

        public TweenInfo(long id)
        {
            Status = TweenStatus.Ready;
            ID = id;
        }

        public TweenInfo(long id, float start, float end, float time, AnimationCurves.EaseType easeTime) : this(id)
        {
            Start = start;
            End = end;
            Time = time;
            EaseType = easeTime;
        }

        public TweenInfo SetOnUpdated(Action<float> onUpdated)
        {
            OnUpdated = onUpdated;
            return this;
        }

        public TweenInfo SetOnCompleted(Action<float> onFinished)
        {
            OnCompleted = onFinished;
            return this;
        }
    }
}
