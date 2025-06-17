using GorillaInfoWatch.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class PageBuilder(params List<(string title, List<ScreenLine>)> pages) : ScreenContent
    {
        public List<(string title, List<ScreenLine> lines)> Pages = pages ?? [];

        public void AddPage(string title = "", params List<ScreenLine> lines)
        {
            Pages.Add((title, lines));
        }

        public override int GetSectionCount()
            => Pages.Sum(page => Mathf.CeilToInt(page.lines.Count / (float)Constants.SectionCapacity));

        public override string GetTitleOfSection(int section)
             => GetSection(section).title;

        public override IEnumerable<ScreenLine> GetLinesAtSection(int section)
            => GetSection(section).lines;

        public (string title, IEnumerable<ScreenLine> lines) GetSection(int section)
        {
            int totalCount = 0;
            foreach (var (title, lines) in Pages)
            {
                int sectionCount = Mathf.CeilToInt(lines.Count / (float)Constants.SectionCapacity);
                if (totalCount + sectionCount > section)
                {
                    int subSection = section - totalCount;
                    return (title, lines.Skip(subSection * Constants.SectionCapacity).Take(Constants.SectionCapacity));
                }
                totalCount += sectionCount;
            }
            Logging.Warning("Empty section");
            return (string.Empty, Enumerable.Empty<ScreenLine>());
        }
    }
}