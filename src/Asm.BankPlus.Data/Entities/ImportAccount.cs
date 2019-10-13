using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus.Data.Entities
{
    public partial class ImportAccount
    {
        public Guid AccountId { get; set; }

        public int ImporterTypeId { get; set; }

        public virtual ImporterType ImporterType { get; set; }

        public virtual Account Account { get; set; }
    }
}
