using GorillaInfoWatch.Behaviours;
using UnityEngine;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_Notification(InfoWatch watch, Menu_StateBase previousState, Notification notification) : Menu_SubState(watch, previousState)
    {
        public readonly Notification notification = notification;

        private float elapsed;

        private bool ended;

        public override void Enter()
        {
            base.Enter();

            Watch.messageMenu.SetActive(true);
        }

        public override void Initialize()
        {
            base.Initialize();

            Watch.messageText.text = notification.Content;
            Watch.messageSlider.value = 1f;

            Watch.redirectIcon.SetActive(notification.Screen is not null);
            Watch.redirectText.text = notification.Screen is null ? string.Empty : notification.Screen.DisplayText;
        }

        public override void Resume()
        {
            base.Resume();

            Watch.stateMachine.SwitchState(previousState);
        }

        public override void Exit()
        {
            base.Exit();

            Watch.messageMenu.SetActive(false);
        }

        public override void Update()
        {
            base.Update();

            if (ended)
                return;

            elapsed += Time.unscaledDeltaTime;

            if (elapsed >= notification.Duration)
            {
                ended = true;
                Watch.stateMachine.SwitchState(previousState);
                return;
            }

            float progress = Mathf.Clamp01(1 - (elapsed / notification.Duration)) * Watch.messageSlider.maxValue;
            Watch.messageSlider.value = Watch.messageSlider.wholeNumbers ? Mathf.CeilToInt(progress) : progress;
        }
    }
}
