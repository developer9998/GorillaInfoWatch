using System;
using System.Threading.Tasks;

namespace GorillaInfoWatch.Models
{
    [Serializable]
    public class Notification(string content, float duration, EWatchSound sound, Notification.ExternalScreen screen = null)
    {
        public string Content { get; } = content;

        public float Duration { get; } = duration;

        public EWatchSound Sound { get; } = sound;

        public ExternalScreen Screen { get; } = screen;

        public string DisplayText = content;

        public DateTime Created = DateTime.Now;

        public bool IsOpened = false;

        public Notification(string head, string body, float duration, EWatchSound sound, ExternalScreen screen = null): this(string.Format("<size=6>{0}:</size><br><b>{1}</b>", head, body), duration, sound, screen)
        {
            DisplayText = $"{head}: {body}";
        }

        public class ExternalScreen(Type screen, string displayText, Task task)
        {
            public Type ScreenType { get; } = screen;
            
            public string DisplayText { get; } = displayText;

            public Task Task { get; } = task;

            public ExternalScreen(Type screen, string displayText, Action action): this(screen, displayText, Task.Run(action))
            {

            }
        }
    }
}
