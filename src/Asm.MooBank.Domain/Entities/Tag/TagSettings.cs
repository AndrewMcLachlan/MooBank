namespace Asm.MooBank.Domain.Entities.Tag;

public class TagSettings
{
    public TagSettings()
    {

    }

    public TagSettings(int id)
    {
        TagId = id;
    }

    internal int TagId { get; set; }

    public bool ApplySmoothing { get; set; }

    public bool ExcludeFromReporting { get; set; }
}
