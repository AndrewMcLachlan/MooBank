namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class InstrumentOwnerConfiguration : IEntityTypeConfiguration<Domain.Entities.Account.InstrumentOwner>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.InstrumentOwner> entity)
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
