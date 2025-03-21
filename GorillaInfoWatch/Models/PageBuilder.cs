using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class PageBuilder : ScreenContent
    {
        public List<(string title, List<ScreenLine> lines)> Pages;

        public PageBuilder(params List<(string title, List<ScreenLine>)> pages)
        {
            Pages = pages ?? [];
        }

        public void AddPage(string title = "", params List<ScreenLine> lines)
        {
            Pages.Add((title, lines));
        }

        public override int GetPageCount()
        {
            return Pages.Sum(page => Mathf.CeilToInt(page.lines.Count / (float)Constants.LinesPerPage));
        }

        public override string GetPageTitle(int page)
        {
            return (page < Pages.Count) ? Pages[page].title : string.Empty;
        }

        public override List<ScreenLine> GetPageLines(int page)
        {
            int overall_page_count = 0;
            foreach(var (title, lines) in Pages)
            {
                int section_page_count = Mathf.CeilToInt(lines.Count / (float)Constants.LinesPerPage);
                if (overall_page_count + section_page_count > page)
                {
                    int sub_page = page - overall_page_count;
                    return lines;
                }
                overall_page_count += section_page_count;
            }
            return Pages.FirstOrDefault().lines ?? [];
        }
    }
}