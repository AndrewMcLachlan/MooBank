﻿namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TagTagConfiguration : IEntityTypeConfiguration<Domain.Entities.Tag.TagTag>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Tag.TagTag> entity)
    {
        entity.ToTable("TransactionTagTransactionTag");
    }
}