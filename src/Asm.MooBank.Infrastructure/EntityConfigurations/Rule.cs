using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class Rule : IEntityTypeConfiguration<Domain.Entities.Instrument.Rule>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Instrument.Rule> entity)
    {
        entity.HasKey(t => t.Id);

        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.Property(e => e.Contains)
                            .IsRequired()
                            .HasMaxLength(50);

        entity.HasMany(p => p.Tags)
                           .WithMany()
                              .UsingEntity<RuleTag>(
                                ttr => ttr.HasOne(ttr2 => ttr2.Tag)
                                          .WithMany()
                                          .HasForeignKey(tt2 => tt2.TagId),
                                ttr => ttr.HasOne(ttr2 => ttr2.Rule)
                                          .WithMany()
                                          .HasForeignKey(ttr2 => ttr2.RuleId),
                                t4 =>
                                {
                                    t4.HasKey(e => new
                                    {
                                        e.RuleId,
                                        e.TagId
                                    });
                                });

        entity.HasOne(e => e.Account)
                    .WithMany(e => e.Rules)
                    .HasForeignKey(e => e.InstrumentId);

    }
}
