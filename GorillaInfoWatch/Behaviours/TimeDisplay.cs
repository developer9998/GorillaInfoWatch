using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using System;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class TimeDisplay : MonoBehaviour, IRelations
    {
        public Relations Relations { get; set; }

        public TMP_Text Text;

        private DateTime Now => DateTime.Now;

        public void LateUpdate()
        {
            Text.text = Now.ToString("h:mm tt");
        }
    }
}
