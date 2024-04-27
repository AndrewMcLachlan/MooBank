namespace Asm.MooBank.Domain.Entities.Tag;
public static class Extensions
{
    public static IEnumerable<Tag> IncludedInReporting(this IEnumerable<Tag> tags) =>
        tags.Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting));
}
