using System;
using System.Collections.Generic;
using System.Linq;
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

		public override void OnScreenOpen()
		{
			this.Draw();
		}

		public void Draw()
		{
			base.LineBuilder = new LineBuilder(null);
			if (PlayerInfoPage.Container == null || PlayerInfoPage.Container.Creator == null || !this.ScoreboardLine)
			{
				base.ShowScreen(typeof(ScoreboardScreen));
				return;
			}
			NetPlayer creator = PlayerInfoPage.Container.Creator;
			VRRig vrrig = GorillaParent.instance.vrrigs.FirstOrDefault((VRRig rig) => rig != null && rig.playerNameVisible == creator.NickName);
			string userId = vrrig.OwningNetPlayer.UserId;
			if (creator == null || !creator.InRoom || creator.IsNull || NetworkSystem.Instance.GetUserID(creator.ActorNumber) == null)
			{
				base.ShowScreen(typeof(ScoreboardScreen));
				return;
			}
			this._normalizedString = (PlayFabAuthenticator.instance.GetSafety() ? creator.DefaultName : creator.NickName).NormalizeName();
			string text = (this._normalizedString != creator.NickName) ? (this._normalizedString.ToUpper() + " (" + creator.NickName + ")") : this._normalizedString.ToUpper();
			if (FriendLib.IsFriend(creator.UserId))
			{
				text = string.Concat(new string[]
				{
					"<color=#",
					ColorUtility.ToHtmlStringRGB(FriendLib.FriendColour),
					">",
					text,
					"</color>"
				});
			}
			base.LineBuilder.AddLine(text, new List<IWidget>(3)
			{
				new WidgetPlayerSwatch(creator, 520f, 90, 90),
				new WidgetPlayerSpeaker(creator, 620f, 100, 100),
				new WidgetSpecialPlayerSwatch(creator, 520f, 80, 70)
			});
			base.LineBuilder.AddLine("User ID: " + creator.UserId, new List<IWidget>());
			base.LineBuilder.AddLine("Master: " + (creator.IsMasterClient ? "Yes" : "No"), new List<IWidget>());
			string str2 = "None";
			if (this.ScoreboardLine.playerVRRig.GetComponent<GorillaSpeakerLoudness>().IsMicEnabled)
			{
				str2 = ((this.ScoreboardLine.playerVRRig.localUseReplacementVoice || this.ScoreboardLine.playerVRRig.remoteUseReplacementVoice) ? "Monke" : "Human");
			}
			base.LineBuilder.AddLine("Voice: " + str2, new List<IWidget>());
			if (vrrig != null)
			{
				base.LineBuilder.AddLine("Color: " + this.ColorCode(vrrig), new List<IWidget>());
			}
			else
			{
				base.LineBuilder.AddLine("Color: <color=red><b>ERROR</b></color>", new List<IWidget>());
			}
			if (!creator.IsLocal)
			{
				base.LineBuilder.AddLines(1, "", new List<IWidget>());
				if (this._usingReportOptions)
				{
					base.LineBuilder.AddLine(string.Format("Type: {0}", this._reportType), new List<IWidget>(1)
					{
						new WidgetSnapSlider(0, 2, (int)this._reportType)
						{
							Command = new Action<int, object[]>(this.OnReportTypeChanged)
						}
					});
					base.LineBuilder.AddLine("Submit", new List<IWidget>(1)
					{
						new WidgetButton(new Action<bool, object[]>(this.OnReportButtonClick), new object[]
						{
							3
						})
					});
					base.LineBuilder.AddLine("Cancel", new List<IWidget>(1)
					{
						new WidgetButton(new Action<bool, object[]>(this.OnReportButtonClick), new object[]
						{
							4
						})
					});
					return;
				}
				base.LineBuilder.AddLine(this.ScoreboardLine.muteButton.isOn ? "Unmute" : "Mute", new List<IWidget>(1)
				{
					new WidgetButton(new Action<bool, object[]>(this.OnMuteButtonClick), new object[]
					{
						creator
					})
				});
				if (FriendLib.FriendCompatible)
				{
					base.LineBuilder.AddLine(FriendLib.IsFriend(creator.UserId) ? "Remove Friend" : "Add Friend", new List<IWidget>(1)
					{
						new WidgetButton(new Action<bool, object[]>(this.OnFriendButtonClick), new object[]
						{
							creator
						})
					});
				}
				base.LineBuilder.AddLines(1, "", new List<IWidget>());
				base.LineBuilder.AddLine(this.ScoreboardLine.reportButton.isOn ? "<color=red>Reported</color>" : "Report", new List<IWidget>(1)
				{
					new WidgetButton(new Action<bool, object[]>(this.OnReportButtonClick), new object[]
					{
						2
					})
				});
			}
		}

		public PlayerInfoPage()
		{
			this.creationDates = new Dictionary<string, string>();
		}

		public string ColorCode(VRRig vrrig)
		{
			return string.Format("{0}, {1}, {2}", Mathf.RoundToInt(vrrig.playerColor.r * 9f), Mathf.RoundToInt(vrrig.playerColor.g * 9f), Mathf.RoundToInt(vrrig.playerColor.b * 9f));
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
			this.Draw();
			base.UpdateLines();
		}

		private void OnFriendButtonClick(bool value, object[] args)
		{
			NetPlayer netPlayer = args[0] as NetPlayer;
			if (netPlayer != null)
			{
				if (FriendLib.IsFriend(netPlayer.UserId))
				{
					FriendLib.RemoveFriend(netPlayer);
				}
				else
				{
					FriendLib.AddFriend(netPlayer);
				}
				this.Draw();
				base.SetLines(false);
			}
		}

		private void OnMuteButtonClick(bool value, object[] args)
		{
			object obj = args[0];
			NetPlayer player = obj as NetPlayer;
			if (player != null && this.ScoreboardLine != null)
			{
				bool isMuted = PlayerPrefs.GetInt(player.UserId, 0) != 0;
				List<GorillaPlayerScoreboardLine> list = (from line in GorillaScoreboardTotalUpdater.allScoreboardLines
				where line.linePlayer.ActorNumber == player.ActorNumber
				select line).ToList<GorillaPlayerScoreboardLine>();
				list.First<GorillaPlayerScoreboardLine>().PressButton(!isMuted, GorillaPlayerLineButton.ButtonType.Mute);
				list.ForEach(delegate(GorillaPlayerScoreboardLine line)
				{
					line.muteButton.isOn = !isMuted;
					line.muteButton.UpdateColor();
				});
				this.Draw();
				base.SetLines(false);
			}
		}

		private void OnReportButtonClick(bool value, object[] args)
		{
			RigContainer container = PlayerInfoPage.Container;
			if (((container != null) ? container.Creator : null) != null)
			{
				NetPlayer player = PlayerInfoPage.Container.Creator;
				List<GorillaPlayerScoreboardLine> list = (from line in GorillaScoreboardTotalUpdater.allScoreboardLines
				where line.linePlayer.ActorNumber == player.ActorNumber
				select line).ToList<GorillaPlayerScoreboardLine>();
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
					this.Draw();
					base.SetLines(false);
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
