using CSCore;
using GorillaGameModes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchModel : MonoBehaviour
    {
        public Relations Relations;

        public bool InRoom => NetworkSystem.Instance.InRoom;
        public bool GamemodeExists => GameMode.ActiveGameMode;

        private Transform timePanel, friendJoinPanel, friendLeavePanel;

        private AudioSource _audioSource;

        private List<Renderer> _renderers = [];
        private List<Graphic> _graphics = [];

        private bool _isVisible = true, _friendPanelInUse = false;

        private float _friendPanelTime;

        public void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            timePanel = transform.Find("Hand Menu/Canvas/Time Display");
            friendJoinPanel = transform.Find("Hand Menu/Canvas/FriendJoined");
            friendLeavePanel = transform.Find("Hand Menu/Canvas/FriendLeft");

            _renderers = [.. GetComponentsInChildren<Renderer>(true)];
            _graphics = [.. GetComponentsInChildren<Graphic>(true)];
        }

        public void SetVisibility(bool visible)
        {
            if (_isVisible != visible)
            {
                _isVisible = visible;

                _renderers.ForEach(renderer => renderer.forceRenderingOff = !_isVisible);
                _graphics.ForEach(text => text.enabled = _isVisible);
            }
        }

        public void ShowFriendPanel(Player player, bool isJoining)
        {
            Transform currentPanel = isJoining ? friendJoinPanel : friendLeavePanel;
            TMP_Text text = currentPanel.Find("Text").GetComponent<TMP_Text>();
            string playerName = PlayFabAuthenticator.instance.GetSafety() ? player.DefaultName : player.NickName;

            text.text = string.Format("<size=60%>A FRIEND HAS {0}:</size>\n<b><color=#{1}>{2}</color></b>", isJoining ? "JOINED" : "LEFT", ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour), playerName.ToNormalizedName().ToUpper());

            _audioSource.PlayOneShot(currentPanel.GetComponent<AudioSource>().clip);
            timePanel.gameObject.SetActive(false);
            friendJoinPanel.gameObject.SetActive(isJoining);
            friendLeavePanel.gameObject.SetActive(!isJoining);

            _friendPanelInUse = true;
            _friendPanelTime = Time.realtimeSinceStartup + 4f;
        }

        public void FixedUpdate()
        {
            SetVisibility(!InRoom || InRoom && !GamemodeExists || InRoom && GamemodeExists && GameMode.ActiveGameMode.GetType() != typeof(GorillaHuntManager));

            if (_friendPanelInUse && Time.realtimeSinceStartup > _friendPanelTime)
            {
                timePanel.gameObject.SetActive(true);
                friendJoinPanel.gameObject.SetActive(false);
                friendLeavePanel.gameObject.SetActive(false);
            }
        }
    }
}
