using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using Photon.Voice.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets;

// anchor of 47.5
// offset of 100
public class WidgetController_PlayerSpeaker(NetPlayer player) : WidgetController
{
    public override Type[] AllowedTypes => [typeof(Widget_Symbol)];
    public override bool? Modification => false;

    public NetPlayer Player = player;

    private Image Image => (Widget as Widget_Symbol).image;

    private RigContainer _rigContainer;
    private Recorder _recorder;
    private bool _isPlayerAutoMuted;

    private Sprite _spriteOpenSpeaker, _spriteMuteSpeaker, _spriteForceMuteSpeaker;

    public override void OnEnable()
    {
        _spriteOpenSpeaker = Symbols.OpenSpeaker.AsSprite();
        _spriteMuteSpeaker = Symbols.MutedSpeaker.AsSprite();
        _spriteForceMuteSpeaker = Symbols.ForceMuteSpeaker.AsSprite();

        if (VRRigCache.Instance.TryGetVrrig(Player, out _rigContainer))
        {
            _isPlayerAutoMuted = PlayerPrefs.HasKey(Player.UserId);
            Image.enabled = false;

            SetSpeakerState();
        }
    }

    public override void Update()
    {
        base.Update();

        if (Player is not null && _rigContainer is not null && _rigContainer.Creator != Player)
        {
            Logging.Info($"PlayerSpeaker for {Player.NickName} will be shut off");
            Enabled = false;
            _rigContainer = null;

            return;
        }

        if (_rigContainer is not null) SetSpeakerState();
    }
    public void SetSpeakerState()
    {
        if (!_isPlayerAutoMuted && _rigContainer.GetIsPlayerAutoMuted())
        {
            if (!Image.enabled || Image.sprite != _spriteForceMuteSpeaker)
            {
                Image.sprite = _spriteForceMuteSpeaker;
                Image.enabled = true;
            }
            return;
        }

        if (_rigContainer.Muted)
        {
            if (!Image.enabled || Image.sprite != _spriteMuteSpeaker)
            {
                Image.sprite = _spriteMuteSpeaker;
                Image.enabled = true;
            }
            return;
        }

        if (_rigContainer.Rig.remoteUseReplacementVoice || _rigContainer.Rig.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn == "FALSE")
        {
            if (_rigContainer.Rig.SpeakingLoudness > _rigContainer.Rig.replacementVoiceLoudnessThreshold && !_rigContainer.ForceMute && !_rigContainer.Muted)
            {
                if (!Image.enabled || Image.sprite != _spriteOpenSpeaker)
                {
                    Image.sprite = _spriteOpenSpeaker;
                    Image.enabled = true;
                }
                return;
            }

            goto HideSpeaker;
        }

        if (_recorder == null) _recorder = NetworkSystem.Instance.LocalRecorder; ;

        if ((_rigContainer.Voice != null && _rigContainer.Voice.IsSpeaking) || (_rigContainer.Rig.isLocal && _recorder.IsCurrentlyTransmitting))
        {
            if (!Image.enabled || Image.sprite != _spriteOpenSpeaker)
            {
                Image.sprite = _spriteOpenSpeaker;
                Image.enabled = true;
            }
            return;
        }

    HideSpeaker:
        if (Image.enabled) Image.enabled = false;
    }
}
