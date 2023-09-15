namespace Asm.MooBank.Domain.Entities.Tag;

public class TagSettings
{
    public TagSettings()
    {

    }

    public TagSettings(int id)
    {
        TransactionTagId = id;
    }

    internal int TransactionTagId { get; set; }

    public bool ApplySmoothing { get; set; }

    public bool ExcludeFromReporting { get; set; }

    //public virtual TransactionTag Tag { get; set; }
}
