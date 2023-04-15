using Asm.MooBank.Domain.Entities.TransactionTags;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TransactionTagConfiguration : IEntityTypeConfiguration<Domain.Entities.TransactionTags.TransactionTag>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TransactionTags.TransactionTag> entity)
    {
        entity.ToTable("TransactionTag");

        entity.Property(e => e.TransactionTagId).ValueGeneratedOnAdd();

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        entity.HasMany(d => d.Tags)
              .WithMany(d => d.TaggedTo)
              .UsingEntity<TransactionTagTransactionTag>(
                t4 => t4.HasOne(t42 => t42.Primary)
                          .WithMany()
                          .HasForeignKey(t42 => t42.PrimaryTransactionTagId),
                t4 => t4.HasOne(ttt2 => ttt2.Secondary)
                          .WithMany()
                          .HasForeignKey(ttt2 => ttt2.SecondaryTransactionTagId),
                t4 =>
                {
                    t4.HasKey(e => new { e.PrimaryTransactionTagId, e.SecondaryTransactionTagId });
                });

        //entity.OwnsOne(e => e.Settings).WithOwner().HasForeignKey("TransactionTagID");
        entity.HasOne(e => e.Settings).WithOne().HasForeignKey<TransactionTag>(e => e.TransactionTagId);
    }
}