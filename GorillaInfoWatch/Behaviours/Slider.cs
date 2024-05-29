using GorillaInfoWatch.Models;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class Slider : MonoBehaviour
    {
        public Relations Relations;

        public Action OnApplied;

        public int Value, Split = 3;

        private BoxCollider _boxCollider;

        private Transform _needle, _min, _max;

        private GorillaTriggerColliderHandIndicator _current;

        private LineSlider _lineSlider;

        public void Awake()
        {
            _needle = transform.Find("Button");
            _min = transform.Find("Min");
            _max = transform.Find("Max");

            _boxCollider = GetComponent<BoxCollider>();

            _boxCollider.isTrigger = true;

            gameObject.SetLayer(UnityLayer.GorillaInteractable);
        }

        public void ApplySlider(LineSlider slider)
        {
            OnApplied = () => slider.RaiseEvent(Value);

            gameObject.SetActive(slider != null);

            if (slider == null)
            {
                _lineSlider = null;
                return;
            }

            if (_lineSlider == null || !slider.Equals(_lineSlider))
            {
                _lineSlider = slider;

                Value = Mathf.Clamp(_lineSlider.InitialValue, 0, slider.Split - 1);
                Split = slider.Split - 1;
            }

            SetNeedlePosition();
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && (_current == null || _current == component) && Relations.Slideable(this))
            {
                Vector3 local = transform.InverseTransformPoint(component.transform.position);
                float clampedPreciseValue = Mathf.Clamp01((local.z - _min.localPosition.z) / (_max.localPosition.z * 2f));
                float oneDivSplit = 1f / Split;
                int targetValue = (int)(Mathf.RoundToInt(clampedPreciseValue / oneDivSplit) * oneDivSplit * Split);
                if (Value != targetValue)
                {
                    Value = targetValue;

                    OnApplied?.Invoke();

                    SetNeedlePosition();

                    if (_current != null) GorillaTagger.Instance.StartVibration(component.isLeftHand, 0.2f, 0.02f);
                }

                if (_current == null)
                {
                    _current = component;
                    Relations.AddSlider(this);
                    GorillaTagger.Instance.StartVibration(component.isLeftHand, 0.25f, 0.05f);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && _current == component)
            {
                _current = null;
                Relations.RemoveSlider(this, component.isLeftHand);
            }
        }

        private void SetNeedlePosition()
        {
            _needle.transform.localPosition = Vector3.Lerp(_min.localPosition, _max.localPosition, Value / (float)Split);
        }
    }
}
