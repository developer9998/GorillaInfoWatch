using GorillaInfoWatch.Behaviours;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_Home(InfoWatch watch) : Menu_StateBase(watch)
    {
        private int frameCount;
        private float timeCount;

        public override void Enter()
        {
            base.Enter();

            Watch.idleMenu.SetActive(true);
        }

        public override void Resume()
        {
            base.Resume();

            frameCount = 0;
            timeCount = 0;
        }

        public override void Exit()
        {
            base.Exit();

            Watch.idleMenu.SetActive(false);
        }

        public override void Update()
        {
            base.Update();

            if (timeCount < 0.5f) // shorter time = less accurate results (zero would defy this logic completely haha)
            {
                timeCount += Time.unscaledDeltaTime;
                frameCount++;
                return;
            }

            InfrequentUpdate();
            timeCount = 0f;
            frameCount = 0;
        }

        public void InfrequentUpdate()
        {
            if (Watch.TimeOffset is float timeOffset)
            {
                DateTime dateTime = DateTime.UtcNow + TimeSpan.FromMinutes(timeOffset);
                string time = dateTime.ToShortTimeString();
                string date = dateTime.ToLongDateString();
                Watch.timeText.text = string.Format("<cspace=0.1em>{0}</cspace><br><size=50%>{1}</size>", time, date);
            }

            Watch.fpsText.text = string.Format("{0} FPS", Watch.Rig.isOfflineVRRig ? Mathf.FloorToInt(frameCount / timeCount) : Watch.Rig.fps);
        }
    }
}
