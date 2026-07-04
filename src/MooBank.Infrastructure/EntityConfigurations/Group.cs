using Asm.MooBank.Infrastructure.ValueConverters;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class GroupConfiguration : IEntityTypeConfiguration<Domain.Entities.Group.Group>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Group.Group> entity)
    {
        // Configure HexColour conversion using dedicated converter
        entity.Property(e => e.Colour)
            .HasConversion<HexColourConverter>()
            .HasColumnType("char(7)")
            .HasMaxLength(7);
    }
}
