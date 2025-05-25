using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public interface IWidgetBehaviour
    {
        public GameObject game_object { get; set; }

        public bool PerformNativeMethods { get; }

        public void Initialize(GameObject game_object);

        public void InvokeUpdate();
    }
}