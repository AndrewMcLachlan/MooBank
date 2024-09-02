using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class RuleConfiguration : IEntityTypeConfiguration<Rule>
{
    public void Configure(EntityTypeBuilder<Rule> entity)
    {
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

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

    }
}
