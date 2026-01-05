using GorillaInfoWatch.Extensions;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.UserInput;

internal class Key : MonoBehaviour
{
    public UserInputBinding Binding;

    public bool IsLargeKey;

    public static event Action<UserInputBinding> OnKeyClicked;

    private bool CanClick => Time.time > (_clickTime + 0.25f);

    private float _clickTime;

    private bool _pressed;

    private Gradient _colour;

    private Renderer _meshRenderer;

    private MaterialPropertyBlock _propertyBlock;

    private readonly int _materialIndex = 1;

    public void Awake()
    {
        _colour = ColourPalette.Button;

        _meshRenderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
        SetColour(false);
    }

    public void OnDisable() => SetColour(false);

    public void Update()
    {
        if (_pressed && CanClick)
        {
            _pressed = false;
            SetColour(false);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator) && CanClick)
        {
            _clickTime = Time.time;
            _pressed = true;
            SetColour(true);

            AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(handIndicator.isLeftHand);
            Sounds sound = IsLargeKey ? Sounds.KeyPushLarge : Sounds.KeyPush;
            handPlayer.PlayOneShot(sound.AsAudioClip(), 0.3f);

            GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            OnKeyClicked?.Invoke(Binding);
        }
    }

    private void SetColour(bool pressed)
    {
        float value = pressed ? 1f : 0f;
        Color colour = _colour.Evaluate(value);
        _propertyBlock.SetColor(ShaderProps._BaseColor, colour);
        _meshRenderer.SetPropertyBlock(_propertyBlock, _materialIndex);
    }
}
