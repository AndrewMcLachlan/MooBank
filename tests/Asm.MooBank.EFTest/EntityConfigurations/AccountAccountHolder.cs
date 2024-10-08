﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class AccountAccountHolder : IEntityTypeConfiguration<Domain.Entities.Account.AccountAccountHolder>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.AccountAccountHolder> entity)
    {
        entity.HasKey(entity => new { entity.AccountId, entity.AccountHolderId });

        entity.HasOne(entity => entity.AccountGroup)
              .WithMany()
              .HasForeignKey(entity => entity.AccountGroupId);


        entity.HasOne(entity => entity.AccountHolder)
              .WithMany()
              .HasForeignKey(entity => entity.AccountHolderId);
    }
}