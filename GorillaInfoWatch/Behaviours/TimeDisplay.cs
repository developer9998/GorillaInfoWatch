using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Behaviours
{
    public class TimeDisplay : MonoBehaviour
    {
        public Text Text;

        private DateTime Now => DateTime.Now;

        public void LateUpdate() => Text.text = Now.ToString("h:mm tt");
    }
}
