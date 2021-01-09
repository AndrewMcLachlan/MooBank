using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asm.BankPlus.Data.EntityConfigurations
{
    public class TransactionExtra : IEntityTypeConfiguration<Entities.Ing.TransactionExtra>
    {
        public void Configure(EntityTypeBuilder<Entities.Ing.TransactionExtra> entity)
        {
            entity.ToTable("TransactionExtra", "ing");

            entity.HasKey(e => e.TransactionId);

            //entity.HasOne(e => e.Transaction).WithOne().HasForeignKey<Entities.Transaction>(e => e.TransactionId);
        }
    }
}
