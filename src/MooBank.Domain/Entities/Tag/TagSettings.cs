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

    [Key]
    internal int TagId { get; set; }

    public bool ApplySmoothing { get; set; }

    public bool ExcludeFromReporting { get; set; }

    /// <summary>
    /// When set, this tag is a budgeting level: budget generation rolls spending from
    /// descendant tags up to the nearest ancestor marked as a budget category.
    /// </summary>
    public bool BudgetCategory { get; set; }
}
