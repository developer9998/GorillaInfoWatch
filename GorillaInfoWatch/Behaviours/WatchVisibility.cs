using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchVisibility : MonoBehaviour
    {
        private List<Renderer> Renderers = new();
        private List<Graphic> Graphics = new(); 

        private bool IsVisible = true;

        public void Start()
        {
            Renderers = GetComponentsInChildren<Renderer>(true).ToList();
            Graphics = GetComponentsInChildren<Graphic>(true).ToList();
        }

        public void SetVisibility(bool visible)
        {
            if (IsVisible != visible)
            {
                IsVisible = visible;

                Renderers.ForEach(renderer => renderer.forceRenderingOff = !IsVisible);
                Graphics.ForEach(text => text.enabled = IsVisible);
            }
        }

        public void FixedUpdate()
        {
            SetVisibility(!(PhotonNetwork.InRoom && GameMode.ActiveGameMode && GameMode.ActiveGameMode.GetType() == typeof(GorillaHuntManager)));
        }
    }
}
