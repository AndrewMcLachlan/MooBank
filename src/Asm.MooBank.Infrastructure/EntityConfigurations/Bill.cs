using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.MooBank.Domain.Entities.Utility;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder
            .HasMany(b => b.Discounts)
            .WithMany(d => d.Bills)
            .UsingEntity<Dictionary<string, object>>(
                "DiscountBill",
                j => j.HasOne<Discount>().WithMany().HasForeignKey("DiscountId"),
                j => j.HasOne<Bill>().WithMany().HasForeignKey("BillId")
            );
    }
}
