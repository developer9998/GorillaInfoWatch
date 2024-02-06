using GorillaInfoWatch.Tools;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Behaviours
{
    public class TimeDisplay : MonoBehaviour
    {
        public Text Text;
        public Configuration Config;

        private DateTime Now => DateTime.Now;

        public void LateUpdate()
        {
            string format = Config.TwFourHour.Value ? "H:mm" : "h:mm tt";
            Text.text = Now.ToString(format);
        }
    }
}
