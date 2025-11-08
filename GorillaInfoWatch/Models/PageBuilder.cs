using GorillaInfoWatch.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class PageBuilder(params List<Section> pages) : InfoContent
    {
        public override int SectionCount => Pages.Sum(page => Mathf.CeilToInt(page.Lines.Count() / (float)Constants.SectionCapacity));

        public List<Section> Pages = pages ?? [];

        public PageBuilder Add(SectionDefinition definition, IEnumerable<SectionLine> lines) => Add(new Section(definition, lines));

        public PageBuilder Add(string title, IEnumerable<SectionLine> lines) => Add(new Section(title, lines));

        public PageBuilder Add(string title, string description, IEnumerable<SectionLine> lines) => Add(new Section(title, description, lines));

        public PageBuilder Add(IEnumerable<SectionLine> lines) => Add(new Section(lines));

        public PageBuilder Add(SectionDefinition definition, ISectionLineProvider lineProvider) => Add(new Section(definition, lineProvider));

        public PageBuilder Add(string title, ISectionLineProvider lineProvider) => Add(new Section(title, lineProvider));

        public PageBuilder Add(string title, string description, ISectionLineProvider lineProvider) => Add(new Section(title, description, lineProvider));

        public PageBuilder Add(ISectionLineProvider lineProvider) => Add(new Section(lineProvider));

        public PageBuilder Add(Section section)
        {
            Pages.Add(section);
            return this;
        }

        public override Section GetSection(int sectionNumber)
        {
            int totalCount = 0;

            foreach (Section section in Pages)
            {
                int lineCount = Mathf.CeilToInt(section.Lines.Count() / (float)Constants.SectionCapacity);

                if ((totalCount + lineCount) > sectionNumber)
                {
                    int linesSkipped = sectionNumber - totalCount;
                    return new(section.Definition, section.Lines.Skip(linesSkipped * Constants.SectionCapacity).Take(Constants.SectionCapacity));
                }

                totalCount += lineCount;
            }

            Logging.Warning("Empty section");
            return new(title: "None", lines: []);
        }
    }
}