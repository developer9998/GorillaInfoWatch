using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Pages
{
    public class ModRoomWarningPage : Page
    {
        public override void OnDisplay()
        {
            if (Plugin.InModdedRoom)
            {
                ShowPage((Type)Parameters[0]);
                return;
            }

            SetHeader("Modded Room", "Warning - Invalid Room");
            AddText("This page has been restricted\nfor modded room usage.");
            AddLines(2);
            AddText("Please enter a modded room if you\nare interested in using this page.");
            SetLines();
        }
    }
}
