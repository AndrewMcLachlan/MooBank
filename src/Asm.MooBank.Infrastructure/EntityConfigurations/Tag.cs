﻿using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TagConfiguration : IEntityTypeConfiguration<Domain.Entities.Tag.Tag>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Tag.Tag> entity)
    {
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.HasMany(d => d.Tags)
              .WithMany(d => d.TaggedTo)
              .UsingEntity<TagTag>(
                t4 => t4.HasOne(t42 => t42.Primary)
                          .WithMany()
                          .HasForeignKey(t42 => t42.PrimaryTagId),
                t4 => t4.HasOne(ttt2 => ttt2.Secondary)
                          .WithMany()
                          .HasForeignKey(ttt2 => ttt2.SecondaryTagId),
                t4 =>
                {
                    t4.HasKey(e => new { e.PrimaryTagId, e.SecondaryTagId });
                });

        //entity.OwnsOne(e => e.Settings).WithOwner().HasForeignKey("TagId");
        entity.HasOne(e => e.Settings).WithOne().HasForeignKey<TagSettings>(e => e.TagId);
    }
}
