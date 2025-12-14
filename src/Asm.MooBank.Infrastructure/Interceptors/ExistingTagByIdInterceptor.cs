using Asm.MooBank.Domain.Entities.Tag;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Asm.MooBank.Infrastructure.Interceptors;


public sealed class ExistingTagByIdInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Apply(DbContext? db)
    {
        if (db is null) return;

        foreach (var entry in db.ChangeTracker.Entries<Tag>())
        {
            // Only fix the specific failure mode: Tag instances that EF thinks are new.
            if ((entry.State == EntityState.Added && entry.Entity.Id != 0) ||
                (entry.State == EntityState.Modified && entry.Entity.Name == null))
            {
                var settings = db.ChangeTracker.Entries<TagSettings>().Single(ts => ts.Entity.TagId == entry.Entity.Id);

                entry.State = EntityState.Unchanged;
                settings.State = EntityState.Unchanged;
            }
        }
    }
}
