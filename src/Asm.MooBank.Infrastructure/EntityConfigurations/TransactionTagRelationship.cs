using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TransactionTagRelationshipConfig : IEntityTypeConfiguration<TransactionTagRelationship>
{
    public void Configure(EntityTypeBuilder<TransactionTagRelationship> entity)
    {
        entity.HasKey(entity => new { entity.Id, entity.ParentId });

        entity.HasOne(t => t.TransactionTag)
            .WithOne()
            .HasForeignKey<TransactionTagRelationship>(t => t.Id);

        entity.HasOne(t => t.ParentTag)
            .WithOne()
            .HasForeignKey<TransactionTagRelationship>(t => t.ParentId);

    }
}
