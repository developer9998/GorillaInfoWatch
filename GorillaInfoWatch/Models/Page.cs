using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Pages;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Page : IPage
    {
        public object[] Parameters { get; set; }

        public List<object> Lines { get; set; }

        public HeaderLine Header { get; set; }

        public Type CallerPage { get; set; }

        public event Action<HeaderLine> OnSetHeaderRequest;

        public event Action<object[]> OnSetLinesRequest;

        public event Action<object[]> OnUpdateLinesRequest;

        public event Action<Type, Type, object[]> OnPageSwitchRequest;

        public Page SetHeader(string heading, string author)
        {
            Header = new HeaderLine(heading, author);
            OnSetHeaderRequest?.Invoke(Header);
            return this;
        }

        public Page AddPlayer(NetPlayer player, LineButton button = null, LineSlider slider = null, LineSymbol symbols = LineSymbol.None) => AddLine(new PlayerLine() { Player = player, Button = button, Slider = slider, Symbols = symbols });

        public Page AddLine(string text = "", LineButton button = null, LineSlider slider = null, LineSymbol symbols = LineSymbol.None) => AddLine(new GenericLine() { Text = text, Button = button, Slider = slider, Symbols = symbols });

        public Page AddLine(object line)
        {
            Lines.Add(line);
            return this;
        }

        public Page AddLines(int repeat, string text = "")
        {
            for (int i = 0; i < repeat; i++)
            {
                AddLine(text);
            }
            return this;
        }

        public Page AddText(string text)
        {
            string[] texts = text.Split('\n');
            foreach (string s in texts)
            {
                int characters = 37;
                int amount = Mathf.CeilToInt((float)s.Length / characters);
                for (int i = 0; i < amount; i++)
                {
                    int start = i * characters;
                    int end = start + characters;
                    if (end > s.Length) end = s.Length;
                    AddLine(s[start..end]);
                }
            }
            return this;
        }

        public void SetLines() => OnSetLinesRequest?.Invoke([.. Lines]);

        public void UpdateLines() => OnUpdateLinesRequest?.Invoke([.. Lines]);

        public void ClearLines() => Lines.Clear();

        public void ReturnToHomePage() => ShowPage(typeof(HomePage));
        public void ReturnToPreviousPage() => ShowPage(CallerPage);

        public void ShowPage(Type type) => ShowPage(type, null);

        public void ShowPage(Type type, object[] parameters) => OnPageSwitchRequest?.Invoke(GetType(), type, parameters);

        public virtual void OnDisplay()
        {

        }

        public virtual void OnClose()
        {

        }
    }
}
