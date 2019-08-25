using System;
using System.Collections.Generic;

namespace Asm.BankPlus.Data.Entities
{
    public partial class AccountAccountHolder
    {
        public Guid AccountId { get; set; }
        public Guid AccountHolderId { get; set; }
    }
}
