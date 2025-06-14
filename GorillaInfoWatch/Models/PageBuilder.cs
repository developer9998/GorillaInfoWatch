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

        public override int SectionCount()
            => Pages.Sum(page => Mathf.CeilToInt(page.lines.Count / (float)Constants.SectionCapacity));

        public override string SectionTitle(int page)
             => GetPageContent(page).page_title;

        public override IEnumerable<ScreenLine> SectionLines(int page)
            => GetPageContent(page).content;

        public (string page_title, IEnumerable<ScreenLine> content) GetPageContent(int page)
        {
            int overall_page_count = 0;
            foreach (var (title, lines) in Pages)
            {
                int section_page_count = Mathf.CeilToInt(lines.Count / (float)Constants.SectionCapacity);
                if (overall_page_count + section_page_count > page)
                {
                    int sub_page = page - overall_page_count;
                    return (title, lines.Skip(sub_page * Constants.SectionCapacity).Take(Constants.SectionCapacity));
                }
                overall_page_count += section_page_count;
            }
            return (string.Empty, Enumerable.Empty<ScreenLine>());
        }
    }
}