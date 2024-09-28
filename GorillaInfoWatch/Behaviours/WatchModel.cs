using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchModel : MonoBehaviour
    {
        public Relations Relations;

        private bool IsHunt => NetworkSystem.Instance.InRoom && (NetworkSystem.Instance.GameModeString.Contains("hunt") || GorillaGameManager.instance?.GetComponent<GorillaHuntManager>() != null);

        private Transform timePanel, friendPanel, huntPanel;

        private TMP_Text _huntText, dayText;
        private Image _material, _hat, _face, _badge, _leftHand, _rightHand;

        private NetPlayer targetPlayer;
        private VRRig targetPlayerRig;
        private CosmeticsController.CosmeticItem _cachedItem;

        private AudioSource _audioSource;

        private bool _friendPanelInUse;

        private float _friendPanelTime;

        public void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            timePanel = transform.Find("Watch Head/Watch GUI/Time Display");
            friendPanel = transform.Find("Watch Head/Watch GUI/Friend Display");
            huntPanel = transform.Find("Watch Head/Watch GUI/Hunt Display");
            _huntText = huntPanel.Find("Text").GetComponent<TMP_Text>();
            _material = huntPanel.Find("Fur").GetComponent<Image>();
            _hat = huntPanel.Find("Hat").GetComponent<Image>();
            _face = huntPanel.Find("Face").GetComponent<Image>();
            _badge = huntPanel.Find("Badge").GetComponent<Image>();
            _leftHand = huntPanel.Find("Image_4").GetComponent<Image>();
            _rightHand = huntPanel.Find("Image_5").GetComponent<Image>();
            dayText = transform.Find("Watch Head/Watch GUI/Time Display/Day").GetComponent<TMP_Text>();
        }

        public void ShowFriendPanel(Player player, bool isJoining)
        {
            TMP_Text text = friendPanel.Find("Text").GetComponent<TMP_Text>();
            string playerName = PlayFabAuthenticator.instance.GetSafety() ? player.DefaultName : player.NickName;

            text.text = string.Format("<size=60%>A FRIEND HAS {0}:</size>\n<b><color=#{1}>{2}</color></b>", isJoining ? "JOINED" : "LEFT", ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour), playerName.NormalizeName().ToUpper());

            _audioSource.PlayOneShot(isJoining ? Relations.FriendJoinClip : Relations.FriendLeftClip);
            timePanel.gameObject.SetActive(false);
            huntPanel.gameObject.SetActive(false);
            friendPanel.gameObject.SetActive(true);

            _friendPanelInUse = true;
            _friendPanelTime = Time.realtimeSinceStartup + 4f;
        }

        public void FixedUpdate()
        {
            if (_friendPanelInUse && Time.realtimeSinceStartup > _friendPanelTime)
            {
                _friendPanelInUse = false;
                timePanel.gameObject.SetActive(!IsHunt);
                huntPanel.gameObject.SetActive(IsHunt);
                friendPanel.gameObject.SetActive(false);
            }

            if (timePanel.gameObject.activeSelf)
            {
                dayText.text = DateTime.Now.ToString("MMMM dd, yyyy");
            }

            if (huntPanel.gameObject.activeSelf)
            {
                if (!IsHunt)
                {
                    huntPanel.gameObject.SetActive(false);
                    timePanel.gameObject.SetActive(true);
                    return;
                }

                GorillaGameManager.instance.TryGetComponent(out GorillaHuntManager hunt);

                if (!hunt) return;

                _huntText.transform.localPosition = Vector3.zero;
                if (!hunt.huntStarted && hunt.waitingToStartNextHuntGame && hunt.currentTarget.Any(player => player.IsLocal) && !hunt.currentHunted.Any(player => player.IsLocal) && hunt.countDownTime == 0)
                {
                    _material.gameObject.SetActive(false);
                    _hat.gameObject.SetActive(false);
                    _face.gameObject.SetActive(false);
                    _badge.gameObject.SetActive(false);
                    _leftHand.gameObject.SetActive(false);
                    _rightHand.gameObject.SetActive(false);
                    _huntText.text = "<b>You won!</b>\n<size=60%>Congrats, hunter!</size>";
                    return;
                }
                if (!hunt.huntStarted && hunt.countDownTime != 0)
                {
                    _material.gameObject.SetActive(false);
                    _hat.gameObject.SetActive(false);
                    _face.gameObject.SetActive(false);
                    _badge.gameObject.SetActive(false);
                    _leftHand.gameObject.SetActive(false);
                    _rightHand.gameObject.SetActive(false);
                    _huntText.text = string.Format("<size=60%>Game starts in..</size>\n<size=180%><b>{0}</b></size>", hunt.countDownTime);
                    return;
                }
                if (!hunt.huntStarted)
                {
                    _material.gameObject.SetActive(false);
                    _hat.gameObject.SetActive(false);
                    _face.gameObject.SetActive(false);
                    _badge.gameObject.SetActive(false);
                    _leftHand.gameObject.SetActive(false);
                    _rightHand.gameObject.SetActive(false);
                    _huntText.text = "<size=60%>Waiting to start</size>";
                    return;
                }

                targetPlayer = hunt.GetTargetOf(PhotonNetwork.LocalPlayer);

                if (targetPlayer == null)
                {
                    _material.gameObject.SetActive(false);
                    _hat.gameObject.SetActive(false);
                    _face.gameObject.SetActive(false);
                    _badge.gameObject.SetActive(false);
                    _leftHand.gameObject.SetActive(false);
                    _rightHand.gameObject.SetActive(false);
                    _huntText.text = "You are dead!\n<size=60%>Tag others to slow them</size>";
                    return;
                }

                targetPlayerRig = hunt.FindPlayerVRRig(targetPlayer);

                if (targetPlayerRig == null) return;

                if (targetPlayerRig.setMatIndex == 0)
                {
                    if (targetPlayerRig.scoreboardMaterial != null)
                    {
                        _material.material = targetPlayerRig.scoreboardMaterial;
                        _material.color = Color.white;
                    }
                    else
                    {
                        _material.material = null;
                        _material.color = targetPlayerRig.playerColor;
                    }
                }
                else
                {
                    _material.material = targetPlayerRig.materialsToChangeTo[targetPlayerRig.setMatIndex];
                    _material.color = Color.white;
                }

                string _hunterWatchText = string.Format("<size=60%>Target Player:</size>\n<b>{0}</b>\n<size=60%>Distance: <b>{1}M</b></size>", targetPlayer.NickName.NormalizeName().ToUpper(), Mathf.CeilToInt((GorillaLocomotion.Player.Instance.headCollider.transform.position - targetPlayerRig.transform.position).magnitude));

                _huntText.text = _hunterWatchText;
                _huntText.transform.localPosition = Vector3.up * -18.1f;

                SetImage(targetPlayerRig.cosmeticSet.items[0].displayName, ref _hat);
                SetImage(targetPlayerRig.cosmeticSet.items[2].displayName, ref _face);
                SetImage(targetPlayerRig.cosmeticSet.items[1].displayName, ref _badge);
                SetImage(GetPrioritizedItemForHand(targetPlayerRig, forLeftHand: true).displayName, ref _leftHand);
                SetImage(GetPrioritizedItemForHand(targetPlayerRig, forLeftHand: false).displayName, ref _rightHand);

                _material.gameObject.SetActive(true);
            }
            else if (IsHunt && !friendPanel.gameObject.activeSelf)
            {
                timePanel.gameObject.SetActive(false);
                huntPanel.gameObject.SetActive(true);
            }
        }

        private void SetImage(string itemDisplayName, ref Image image)
        {
            _cachedItem = CosmeticsController.instance.GetItemFromDict(CosmeticsController.instance.GetItemNameFromDisplayName(itemDisplayName));
            if (_cachedItem.displayName != "NOTHING" && targetPlayer != null)
            {
                image.gameObject.SetActive(true);
                image.sprite = _cachedItem.itemPicture;
                return;
            }
            image.gameObject.SetActive(false);
        }

        private CosmeticsController.CosmeticItem GetPrioritizedItemForHand(VRRig targetRig, bool forLeftHand)
        {
            if (forLeftHand)
            {
                CosmeticsController.CosmeticItem cosmeticItem = targetRig.cosmeticSet.items[7];
                if (cosmeticItem.displayName != "null")
                {
                    return cosmeticItem;
                }
                cosmeticItem = targetRig.cosmeticSet.items[4];
                if (cosmeticItem.displayName != "null")
                {
                    return cosmeticItem;
                }
                return targetRig.cosmeticSet.items[5];
            }
            else
            {
                CosmeticsController.CosmeticItem cosmeticItem = targetRig.cosmeticSet.items[8];
                if (cosmeticItem.displayName != "null")
                {
                    return cosmeticItem;
                }
                cosmeticItem = targetRig.cosmeticSet.items[3];
                if (cosmeticItem.displayName != "null")
                {
                    return cosmeticItem;
                }
                return targetRig.cosmeticSet.items[6];
            }
        }
    }
}
