using System;
using System.Collections.Generic;

namespace Asm.MooBank.Data.Entities
{
    public partial class AccountAccountHolder
    {
        public Guid AccountId { get; set; }
        public Guid AccountHolderId { get; set; }

        public virtual Account Account { get; set; }

        public virtual AccountHolder AccountHolder { get; set; }
    }
}
