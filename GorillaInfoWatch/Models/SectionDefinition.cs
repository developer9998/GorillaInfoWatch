namespace GorillaInfoWatch.Models;

public class SectionDefinition(string title, string description = "")
{
    public string Title { get; } = title;

    public string Description { get; } = description;
}