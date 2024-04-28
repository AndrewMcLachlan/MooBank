﻿using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class InstrumentViewerConfiguration : IEntityTypeConfiguration<InstrumentViewer>
{
    public void Configure(EntityTypeBuilder<InstrumentViewer> entity)
    {
        entity.HasKey(entity => new { entity.InstrumentId, entity.UserId });

        entity.HasOne(entity => entity.Instrument)
            .WithMany(account => account.Viewers)
                .HasForeignKey(entity => entity.InstrumentId);

        entity.HasOne(entity => entity.Group)
              .WithMany()
              .HasForeignKey(entity => entity.GroupId);

        entity.HasOne(entity => entity.User)
              .WithMany()
              .HasForeignKey(entity => entity.UserId);
    }
}
