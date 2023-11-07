using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TransactionTagRelationshipConfig : IEntityTypeConfiguration<TagRelationship>
{
    public void Configure(EntityTypeBuilder<TagRelationship> entity)
    {
        entity.HasKey(entity => new { entity.Id, entity.ParentId });

        entity.HasOne(t => t.TransactionTag)
            .WithOne()
            .HasForeignKey<TagRelationship>(t => t.Id);

        entity.HasOne(t => t.ParentTag)
            .WithOne()
            .HasForeignKey<TagRelationship>(t => t.ParentId);

    }
}
