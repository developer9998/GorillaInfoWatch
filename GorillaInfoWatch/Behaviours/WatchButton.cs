using GorillaInfoWatch.Models;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchButton : MonoBehaviour
    {
        public Main Main;
        public InputType Type;

        private Color IdleColour, PressedColour;

        private BoxCollider Collider;
        private MeshRenderer Renderer;

        private readonly float Debounce = 0.15f;
        private float TouchTime;
        private bool Pressed;

        public void Start()
        {
            IdleColour = new Color32(217, 216, 221, 255);

            Color.RGBToHSV(IdleColour, out float H, out float S, out float _);
            PressedColour = Color.HSVToRGB(H, S, 0.6f);

            Renderer = GetComponent<MeshRenderer>();
            Renderer.material.color = IdleColour;

            Collider = GetComponent<BoxCollider>();
        }

        public void Update()
        {
            if (Pressed && Time.time >= (TouchTime + Debounce))
            {
                Pressed = false;
                BumpOut();
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator) && !handIndicator.isLeftHand && !Pressed)
            {
                Pressed = true;
                BumpIn();

                TouchTime = Time.time;

                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                Main.PressButton(this, handIndicator.isLeftHand);
            }
        }

        public void BumpIn()
        {
            Renderer.material.color = PressedColour;

            transform.localScale -= new Vector3(0f, 0f, 0.001f);
            transform.localPosition += new Vector3(0f, 0f, 0.0005f);

            Collider.center -= new Vector3(0f, 0f, 0.17f);
            Collider.size += new Vector3(0f, 0f, 0.17f);
        }

        public void BumpOut()
        {
            Renderer.material.color = IdleColour;

            transform.localScale += new Vector3(0f, 0f, 0.001f);
            transform.localPosition -= new Vector3(0f, 0f, 0.0005f);

            Collider.center += new Vector3(0f, 0f, 0.17f);
            Collider.size -= new Vector3(0f, 0f, 0.17f);
        }
    }
}
