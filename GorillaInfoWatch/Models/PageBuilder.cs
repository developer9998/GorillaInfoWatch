using System.Collections.Generic;
using System.Linq;
using GorillaInfoWatch.Tools;
using UnityEngine;

namespace GorillaInfoWatch.Models;

public class PageBuilder(params List<(string title, List<InfoLine>)> pages) : InfoContent
{
    public List<(string title, List<InfoLine> lines)> Pages = pages ?? [];

    public override int SectionCount =>
            Pages.Sum(page => Mathf.CeilToInt(page.lines.Count / (float)Constants.SectionCapacity));

    public PageBuilder AddPage(params List<InfoLine> lines) => AddPage(string.Empty, lines);

    public PageBuilder AddPage(string title = "", params List<InfoLine> lines)
    {
        Pages.Add((title, lines));

        return this;
    }

    public override string GetTitleOfSection(int section) => GetSection(section).title;

    public override IEnumerable<InfoLine> GetLinesAtSection(int section) => GetSection(section).lines;

    public (string title, IEnumerable<InfoLine> lines) GetSection(int section)
    {
        int totalCount = 0;

        foreach ((string title, List<InfoLine> lines) in Pages)
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

        return (string.Empty, Enumerable.Empty<InfoLine>());
    }
}