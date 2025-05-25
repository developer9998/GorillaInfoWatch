using System;
using System.Collections.Generic;
﻿using System.Linq;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaNetworking;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{

	public class PlayerInfoPage : WatchScreen
	{
		public override string Title
		{
			get
			{
				return "Player Inspector";
			}
		}

		private GorillaPlayerScoreboardLine ScoreboardLine
		{
			get
			{
				return GorillaScoreboardTotalUpdater.allScoreboardLines.FirstOrDefault((GorillaPlayerScoreboardLine line) => line.playerVRRig == PlayerInfoPage.Container.Rig);
			}
		}

		public PlayerInfoPage()
		{
			this.creationDates = new Dictionary<string, string>();
		}

        public override ScreenContent GetContent()
        {
            if (Container == null || Container.Creator == null || !this.ScoreboardLine)
            {
				SetScreen<ScoreboardScreen>();
				return null;
            }

            VRRig vrrig = Container.Rig;
            NetPlayer creator = Container.Creator;

            if (creator == null || !creator.InRoom || creator.IsNull || NetworkSystem.Instance.GetUserID(creator.ActorNumber) == null)
            {
                SetScreen<ScoreboardScreen>();
                return null;
            }

            string userId = creator.UserId;

            this._normalizedString = (PlayFabAuthenticator.instance.GetSafety() ? creator.DefaultName : creator.NickName).NormalizeName();
            string text = (this._normalizedString != creator.NickName) ? (this._normalizedString.ToUpper() + " (" + creator.NickName + ")") : this._normalizedString.ToUpper();
            
			if (FriendLib.IsFriend(creator.UserId))
            {
                text = string.Concat(
                [
                    "<color=#",
                    ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour),
                    ">",
                    text,
                    "</color>"
                ]);
            }

			LineBuilder lines = new();

            lines.AddLine(text, new List<IWidget>(3)
            {
                new WidgetPlayerSwatch(creator, 520f, 90, 90),
                new WidgetPlayerSpeaker(creator, 620f, 100, 100),
                new WidgetSpecialPlayerSwatch(creator, 520f, 80, 70)
            });

            lines.AddLine("User ID: " + creator.UserId, new List<IWidget>());
            lines.AddLine("Master: " + (creator.IsMasterClient ? "Yes" : "No"), new List<IWidget>());
            string str2 = "None";
            if (this.ScoreboardLine.playerVRRig.GetComponent<GorillaSpeakerLoudness>().IsMicEnabled)
            {
                str2 = ((this.ScoreboardLine.playerVRRig.localUseReplacementVoice || this.ScoreboardLine.playerVRRig.remoteUseReplacementVoice) ? "Monke" : "Human");
            }
            lines.AddLine("Voice: " + str2, new List<IWidget>());
            if (vrrig != null)
            {
                lines.AddLine("Colour (0-9): " + this.ColorCode9(vrrig), new List<IWidget>());
                lines.AddLine("Colour (0-255): " + this.ColorCode255(vrrig), new List<IWidget>());
            }
            else
            {
                lines.AddLine("Colour (0-9): <color=red><b>ERROR</b></color>", new List<IWidget>());
                lines.AddLine("Colour (0-255): <color=red><b>ERROR</b></color>", new List<IWidget>());
            }
            if (!creator.IsLocal)
            {
                lines.AddLines(1, "", new List<IWidget>());
                if (this._usingReportOptions)
                {
                    lines.AddLine(string.Format("Type: {0}", this._reportType), new List<IWidget>(1)
                    {
                        new WidgetSnapSlider(0, 2, (int)this._reportType)
                        {
                            Command = new Action<int, object[]>(this.OnReportTypeChanged)
                        }
                    });
                    lines.AddLine("Submit", new List<IWidget>(1)
                    {
                        new WidgetButton(new Action<bool, object[]>(this.OnReportButtonClick),
                        [
                            3
                        ])
                    });
                    lines.AddLine("Cancel", new List<IWidget>(1)
                    {
                        new WidgetButton(new Action<bool, object[]>(this.OnReportButtonClick),
                        [
                            4
                        ])
                    });

					return lines;
                }

                lines.AddLine(this.ScoreboardLine.muteButton.isOn ? "Unmute" : "Mute", new List<IWidget>(1)
                {
                    new WidgetButton(new Action<bool, object[]>(this.OnMuteButtonClick),
                    [
                        creator
                    ])
                });

                if (FriendLib.FriendCompatible)
                {
                    lines.AddLine(FriendLib.IsFriend(creator.UserId) ? "Remove Friend" : "Add Friend", new List<IWidget>(1)
                    {
                        new WidgetButton(new Action<bool, object[]>(this.OnFriendButtonClick),
                        [
                            creator
                        ])
                    });
                }

                lines.AddLines(1, "", new List<IWidget>());
                lines.AddLine(this.ScoreboardLine.reportButton.isOn ? "<color=red>Reported</color>" : "Report", new List<IWidget>(1)
                {
                    new WidgetButton(new Action<bool, object[]>(this.OnReportButtonClick),
                    [
                        2
                    ])
                });
            }

			return lines;
        }

        public string ColorCode9(VRRig vrrig)
		{
			return string.Format("{0}, {1}, {2}", Mathf.RoundToInt(vrrig.playerColor.r * 9f), Mathf.RoundToInt(vrrig.playerColor.g * 9f), Mathf.RoundToInt(vrrig.playerColor.b * 9f));
		}

		public string ColorCode255(VRRig vrrig)
		{
			return string.Format("{0}, {1}, {2}", Mathf.RoundToInt(vrrig.playerColor.r * 255f), Mathf.RoundToInt(vrrig.playerColor.g * 255f), Mathf.RoundToInt(vrrig.playerColor.b * 255f));
		}

		public override void OnScreenClose()
		{
			this._usingReportOptions = false;
			this._reportType = GorillaPlayerLineButton.ButtonType.Toxicity;
		}

		private void OnReportTypeChanged(int value, object[] args)
		{
			GorillaPlayerLineButton.ButtonType reportType;
			switch (value)
			{
			case 0:
				reportType = GorillaPlayerLineButton.ButtonType.Toxicity;
				break;
			case 1:
				reportType = GorillaPlayerLineButton.ButtonType.HateSpeech;
				break;
			case 2:
				reportType = GorillaPlayerLineButton.ButtonType.Cheating;
				break;
			default:
				reportType = GorillaPlayerLineButton.ButtonType.Toxicity;
				break;
			}
			this._reportType = reportType;
			base.SetText();
		}

		private void OnFriendButtonClick(bool value, object[] args)
		{
            if (args.ElementAtOrDefault(0) is NetPlayer netPlayer)
            {
                if (FriendLib.IsFriend(netPlayer.UserId))
                {
                    FriendLib.RemoveFriend(netPlayer);
                }
                else
                {
                    FriendLib.AddFriend(netPlayer);
                }
                base.SetContent(false);
            }
        }

		private void OnMuteButtonClick(bool value, object[] args)
		{
			object obj = args.ElementAtOrDefault(0);
            if (obj is NetPlayer player && this.ScoreboardLine != null)
            {
                bool isMuted = PlayerPrefs.GetInt(player.UserId, 0) != 0;
				var lines = GorillaScoreboardTotalUpdater.allScoreboards.SelectMany(scoreboard => scoreboard.lines).Where(line => line.linePlayer == player);
				lines.ForEach(line =>
				{
					line.muteButton.isOn = !isMuted;
					line.PressButton(!isMuted, GorillaPlayerLineButton.ButtonType.Mute);
                });

                SetContent(false);
            }
        }

		private void OnReportButtonClick(bool value, object[] args)
		{
			RigContainer container = Container;
			if ((container?.Creator) != null)
			{
				NetPlayer player = PlayerInfoPage.Container.Creator;
				List<GorillaPlayerScoreboardLine> list = [.. (from line in GorillaScoreboardTotalUpdater.allScoreboardLines
				where line.linePlayer.ActorNumber == player.ActorNumber
				select line)];
				object obj = args[0];
				int num = -1;
				bool flag;
				if (obj is int)
				{
					num = (int)obj;
					flag = true;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					switch (num)
					{
					case 2:
						list.ForEach(delegate(GorillaPlayerScoreboardLine line)
						{
							line.PressButton(false, GorillaPlayerLineButton.ButtonType.Report);
						});
						list.ForEach(delegate(GorillaPlayerScoreboardLine line)
						{
							line.reportButton.gameObject.SetActive(true);
						});
						this._usingReportOptions = true;
						this._reportType = GorillaPlayerLineButton.ButtonType.Toxicity;
						break;
					case 3:
						list.First<GorillaPlayerScoreboardLine>().PressButton(false, this._reportType);
						list.ForEach(delegate(GorillaPlayerScoreboardLine line)
						{
							line.reportButton.isOn = true;
							line.reportButton.UpdateColor();
							line.reportButton.gameObject.SetActive(true);
							line.toxicityButton.SetActive(false);
							line.hateSpeechButton.SetActive(false);
							line.cheatingButton.SetActive(false);
							line.cancelButton.SetActive(false);
						});
						this._usingReportOptions = false;
						break;
					case 4:
						list.First<GorillaPlayerScoreboardLine>().PressButton(false, GorillaPlayerLineButton.ButtonType.Cancel);
						list.ForEach(delegate(GorillaPlayerScoreboardLine line)
						{
							line.reportInProgress = false;
							line.reportButton.gameObject.SetActive(true);
							line.toxicityButton.SetActive(false);
							line.hateSpeechButton.SetActive(false);
							line.cheatingButton.SetActive(false);
							line.cancelButton.SetActive(false);
						});
						this._usingReportOptions = false;
						break;
					}
					base.SetContent(false);
				}
			}
		}

		public static RigContainer Container;
		private readonly Dictionary<string, string> creationDates;
		private bool _usingReportOptions;
		private GorillaPlayerLineButton.ButtonType _reportType = GorillaPlayerLineButton.ButtonType.Toxicity;
		private string _normalizedString;
	}
}
