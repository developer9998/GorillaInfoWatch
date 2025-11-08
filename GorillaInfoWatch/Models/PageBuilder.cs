using GorillaInfoWatch.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class PageBuilder(params List<(string title, string description, List<InfoLine>)> pages) : InfoContent
    {
        public override int SectionCount => Pages.Sum(page => Mathf.CeilToInt(page.lines.Count / (float)Constants.SectionCapacity));

        public List<(string title, string description, List<InfoLine> lines)> Pages = pages ?? [];

        public PageBuilder Add(LineBuilder lines, string title = "") => Add(title, lines.Lines);

        public PageBuilder Add(string title = "", params List<InfoLine> lines) => Add(title, string.Empty, lines: lines);

        public PageBuilder Add(string title = "", string description = "", params List<InfoLine> lines)
        {
            Pages.Add((title, description, lines));
            return this;
        }

        public override string GetTitleOfSection(int section) => GetSection(section).title;

        public override string GetDescriptionOfSection(int section) => GetSection(section).description;

        public override IEnumerable<InfoLine> GetLinesAtSection(int section) => GetSection(section).lines;

        public (string title, string description, IEnumerable<InfoLine> lines) GetSection(int section)
        {
            int totalCount = 0;

            foreach (var (title, description, lines) in Pages)
            {
                int sectionCount = Mathf.CeilToInt(lines.Count / (float)Constants.SectionCapacity);

                if (totalCount + sectionCount > section)
                {
                    int subSection = section - totalCount;
                    return (title, description, lines.Skip(subSection * Constants.SectionCapacity).Take(Constants.SectionCapacity));
                }

                totalCount += sectionCount;
            }

            Logging.Warning("Empty section");
            return (string.Empty, string.Empty, Enumerable.Empty<InfoLine>());
        }
    }
}