using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TagRelationshipConfig : IEntityTypeConfiguration<TagRelationship>
{
    public void Configure(EntityTypeBuilder<TagRelationship> entity)
    {
        entity.HasKey(entity => new { entity.Id, entity.ParentId });

        entity.HasOne(t => t.Tag)
            .WithOne()
            .HasForeignKey<TagRelationship>(t => t.Id);

        entity.HasOne(t => t.ParentTag)
            .WithOne()
            .HasForeignKey<TagRelationship>(t => t.ParentId);

    }
}
