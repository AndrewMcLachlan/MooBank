using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TransactionTagRule : IEntityTypeConfiguration<Domain.Entities.Account.TransactionTagRule>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.TransactionTagRule> entity)
    {
        entity.HasKey(t => t.TransactionTagRuleId);

        entity.Property(e => e.TransactionTagRuleId).ValueGeneratedOnAdd();

        entity.Property(e => e.Contains)
                            .IsRequired()
                            .HasMaxLength(50);

        entity.HasMany(p => p.TransactionTags)
                           .WithMany()
                              .UsingEntity<TransactionTagRuleTransactionTag>(
                                ttr => ttr.HasOne(ttr2 => ttr2.TransactionTag)
                                          .WithMany()
                                          .HasForeignKey(tt2 => tt2.TransactionTagId),
                                ttr => ttr.HasOne(ttr2 => ttr2.TransactionTagRule)
                                          .WithMany()
                                          .HasForeignKey(ttr2 => ttr2.TransactionTagRuleId),
                                t4 =>
                                {
                                    t4.HasKey(e => new
                                    {
                                        e.TransactionTagRuleId,
                                        e.TransactionTagId
                                    });
                                });

        entity.HasOne(e => e.Account)
                    .WithMany()
                    .HasForeignKey(e => e.AccountId);

    }
}