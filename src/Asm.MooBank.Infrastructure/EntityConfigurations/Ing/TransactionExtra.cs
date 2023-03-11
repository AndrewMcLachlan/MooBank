﻿namespace Asm.MooBank.Infrastructure.EntityConfigurations.Ing;

public class TransactionExtra : IEntityTypeConfiguration<Data.Entities.Ing.TransactionExtra>
{
    public void Configure(EntityTypeBuilder<Data.Entities.Ing.TransactionExtra> entity)
    {
        entity.ToTable("TransactionExtra", "ing");

        entity.HasKey(e => e.TransactionId);

        //entity.HasOne(e => e.Transaction).WithOne().HasForeignKey<Entities.Transaction>(e => e.TransactionId);
    }
}
