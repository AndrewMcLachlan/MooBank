using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class InstrumentOwnerConfiguration : IEntityTypeConfiguration<InstrumentOwner>
{
    public void Configure(EntityTypeBuilder<InstrumentOwner> entity)
    {
        entity.HasKey(entity => new { entity.InstrumentId, entity.UserId });

        entity.HasOne(entity => entity.Group)
              .WithMany()
              .HasForeignKey(entity => entity.GroupId);


        /*entity.HasOne(entity => entity.User)
              .WithMany()
              .HasForeignKey(entity => entity.UserId);*/
    }
}
