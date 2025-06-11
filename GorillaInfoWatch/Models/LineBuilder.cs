using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorillaInfoWatch.Models.Widgets;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class LineBuilder(List<ScreenLine> lines) : ScreenContent
    {
        public List<ScreenLine> Lines = lines ?? [];

        public LineBuilder() : this([])
        {

        }

        public LineBuilder(string content) : this(content.Split(Environment.NewLine).Select(line => new ScreenLine(line)).ToList())
        {

        }

        public void AddLine()
        {
            Lines.Add(new(string.Empty, []));
        }

        public void AddLine(string text, params List<Widget> widgets)
        {
            Lines.Add(new(text, widgets));
        }

        public void AddLines(int amount, string text = "", params List<Widget> widgets)
        {
            Lines.AddRange(Enumerable.Repeat<ScreenLine>(new(text, widgets), amount));
        }

        public void AddText(string text) // something something monkestatistics
        {
            string[] texts = text.Split('\n');
            foreach (string s in texts)
            {
                int characters = 37; // TODO: update this if needed, *might* not be accurate
                int amount = Mathf.CeilToInt((float)s.Length / characters);
                for (int i = 0; i < amount; i++)
                {
                    int start = i * characters;
                    int end = start + characters;
                    if (end > s.Length) end = s.Length;
                    AddLine(s[start..end]);
                }
            }
        }

        public static implicit operator LineBuilder(StringBuilder str)
        {
            string[] split = str.ToString().Split(Environment.NewLine);
            return new(lines: [.. split.Select(str => new ScreenLine(str))]);
        }

        public static implicit operator List<ScreenLine>(LineBuilder line_builder)
        {
            return line_builder.Lines;
        }

        public override int GetPageCount()
            => Mathf.CeilToInt(Lines.Count / (float)Constants.LinesPerPage);

        public override IEnumerable<ScreenLine> GetPageLines(int page)
            => Lines.Skip(page * Constants.LinesPerPage).Take(Constants.LinesPerPage);

        public override string GetPageTitle(int page)
            => string.Empty;
    }
}