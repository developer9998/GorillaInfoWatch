using GorillaInfoWatch.Models;
using System;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class Button : MonoBehaviour
    {
        public Relations Relations;

        public Action OnPressed;

        private Color _baseColour, _activatedColour;

        private BoxCollider _boxCollider;
        private MeshRenderer _meshRenderer;

        private bool _bumped, _toggle;

        private LineButton _lineButton;

        public void Awake()
        {
            _baseColour = new Color32(191, 188, 170, 255);
            _activatedColour = new Color32(132, 131, 119, 255);

            _meshRenderer = GetComponent<MeshRenderer>();

            _meshRenderer.materials[1] = new Material(_meshRenderer.materials[1]) { color = _baseColour };

            _boxCollider = GetComponent<BoxCollider>();

            _boxCollider.isTrigger = true;

            gameObject.SetLayer(UnityLayer.GorillaInteractable);
        }

        public void ApplyButton(LineButton lineButton)
        {
            _lineButton = lineButton;
            OnPressed = () => _lineButton.RaiseEvent(_toggle ? _bumped : true);

            gameObject.SetActive(_lineButton != null);

            if (_lineButton == null) return;

            _toggle = lineButton.UseToggle;

            GetComponentInChildren<TextMeshProUGUI>().text = lineButton.Text;

            if (_toggle && lineButton.InitialValue && !_bumped)
            {
                _bumped = true;
                _meshRenderer.materials[1].color = _activatedColour;
            }
            else if (_toggle && !lineButton.InitialValue && _bumped)
            {
                _bumped = false;
                _meshRenderer.materials[1].color = _baseColour;
            }

            if (_bumped && !_toggle)
            {
                _bumped = false;
                _meshRenderer.materials[1].color = _baseColour;
            }
        }

        public void FixedUpdate()
        {
            if (_bumped && !_toggle && Relations.Pressable())
            {
                _bumped = false;
                _meshRenderer.materials[1].color = _baseColour;
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && Relations.Pressable())
            {
                _bumped = !_toggle || !_bumped;
                _meshRenderer.materials[1].color = _bumped ? _activatedColour : _baseColour;

                OnPressed?.Invoke(); // addon functions
                Relations.Press(this, component.isLeftHand); // base functions
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration); // haptic
            }
        }

        public void OnDisable()
        {
            if (_bumped && !_toggle)
            {
                _bumped = false;
                _meshRenderer.materials[1].color = _baseColour;
            }
        }
    }
}
