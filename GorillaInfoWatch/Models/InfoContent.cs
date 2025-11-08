namespace GorillaInfoWatch.Models;

public abstract class InfoContent
{
    public abstract int SectionCount { get; }
    public abstract Section GetSection(int sectionNumber);
}