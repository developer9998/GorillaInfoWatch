using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using Photon.Realtime;
using System;
using System.Text;

namespace GorillaInfoWatch.Tabs
{
    public class ReportWindow : Window
    {
        private ItemHandler ItemHandler;

        private Player Player;
        private GorillaPlayerScoreboardLine Line;

        public override void OnWindowDisplayed(object[] Parameters)
        {
            Player = (Player)Parameters[0];
            Line = (GorillaPlayerScoreboardLine)Parameters[1];

            ItemHandler = new ItemHandler(3);
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();
            str.AppendLine($"- Report -".AlignCenter(Constants.Width)).AppendLine();

            str.Append("Reporting ").AppendLine(Player.NickName).AppendLine();
            str.AppendItem("Cheating", 0, ItemHandler);
            str.AppendItem("Toxicity", 1, ItemHandler);
            str.AppendItem("Hate Speech", 2, ItemHandler);

            str.AppendLine().Append("<color=red>These changes cannot be undone after confirming the report.</color>");

            SetText(str.ToString());
        }

        public override void OnButtonPress(ButtonType type)
        {
            if (ItemHandler.HandleButton(type))
            {
                OnScreenRefresh();
                return;
            }

            switch (type)
            {
                case ButtonType.Enter:
                    GorillaPlayerLineButton.ButtonType reportType = ItemHandler.CurrentEntry switch
                    {
                        0 => GorillaPlayerLineButton.ButtonType.Cheating,
                        1 => GorillaPlayerLineButton.ButtonType.Toxicity,
                        2 => GorillaPlayerLineButton.ButtonType.HateSpeech,
                        _ => throw new IndexOutOfRangeException()
                    };
                    Line.PressButton(false, GorillaPlayerLineButton.ButtonType.Report);
                    Line.PressButton(false, reportType);
                    DisplayWindow<PlayerWindow>();
                    OnScreenRefresh();
                    break;
                case ButtonType.Back:
                    DisplayWindow<PlayerWindow>();
                    break;
            }
        }
    }
}
