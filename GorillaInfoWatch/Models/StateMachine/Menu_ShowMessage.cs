using GorillaInfoWatch.Behaviours;
using UnityEngine;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_ShowMessage(InfoWatch watch, Menu_StateBase previousState, string content, float duration, WatchScreen redirect) : Menu_SubState(watch, previousState)
    {
        protected string content = content;
        protected float duration = duration;
        protected WatchScreen redirect = redirect;

        readonly bool hasRedirect = redirect is not null;

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

            Watch.messageText.text = content;
            Watch.messageSlider.value = 1f;

            Watch.redirectIcon.SetActive(hasRedirect);
            Watch.redirectText.text = hasRedirect ? redirect.Title : string.Empty;
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

            if (elapsed >= duration)
            {
                ended = true;
                Watch.RedirectScreen = null;
                Watch.RedirectPrep = null;
                Watch.stateMachine.SwitchState(previousState);
                return;
            }

            float progress = Mathf.Clamp01(1 - (elapsed / duration)) * Watch.messageSlider.maxValue;
            Watch.messageSlider.value = Watch.messageSlider.wholeNumbers ? Mathf.CeilToInt(progress) : progress;
        }
    }
}
