using Asm.MooBank.Domain.Entities.Tag;

namespace System.Linq;
public static class Extensions
{
    public static IEnumerable<Tag> IncludedInReporting(this IEnumerable<Tag> tags) =>
        tags.Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting));
}
