using GorillaInfoWatch.Models.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class LineBuilder(List<ScreenLine> lines) : ScreenContent
    {
        public List<ScreenLine> Lines = lines ?? [];

        public LineBuilder() : this([])
        {

        }

        public LineBuilder(string content) : this([.. content.Split(Environment.NewLine).Select(line => new ScreenLine(line))])
        {

        }

        public void Skip()
            => Add(string.Empty);

        public void Add(string text, params List<Widget_Base> widgets)
            => Lines.Add(new(text, widgets));

        public void Insert(int position, string text, params List<Widget_Base> widgets)
            => Lines.Insert(position, new(text, widgets));

        public void Repeat(int amount, string text = "", params List<Widget_Base> widgets)
            => Lines.AddRange(Enumerable.Repeat<ScreenLine>(new(text, widgets), amount));

        public static implicit operator List<ScreenLine>(LineBuilder lines)
        {
            return lines.Lines;
        }

        public override int GetSectionCount()
            => Mathf.CeilToInt(Lines.Count / (float)Constants.SectionCapacity);

        public override IEnumerable<ScreenLine> GetLinesAtSection(int section)
            => Lines.Skip(section * Constants.SectionCapacity).Take(Constants.SectionCapacity);

        public override string GetTitleOfSection(int section)
            => string.Empty;
    }
}