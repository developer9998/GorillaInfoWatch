using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaInfoWatch.Behaviours.UI.Widgets;

public class PromptButton : MonoBehaviour
{
    public Widget_PromptButton Widget
    {
        get => _widget;
        set => SetWidget(value);
    }

    private Widget_PromptButton _widget;

    public void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        gameObject.SetLayer(UnityLayer.GorillaInteractable);
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out HandIndicator handIndicator) && handIndicator.isLeftHand != Watch.LocalWatch.InLeftHand && Main.Instance.CheckInteractionInterval(WatchInteractionSource.Widget, 0.25f))
        {
            AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(handIndicator.isLeftHand);
            handPlayer.PlayOneShot(Sounds.widgetButton.AsAudioClip(), 0.2f);

            GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            UserInput.Activate(Widget.Input, Widget.Keyboard, Widget.Limit, Widget.Submit);
        }
    }

    private void SetWidget(Widget_PromptButton widget)
    {
        _widget = widget;
        gameObject.SetActive(widget != null);
    }
}
