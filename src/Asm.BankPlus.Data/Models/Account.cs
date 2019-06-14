using System;
using System.Collections.Generic;

namespace Asm.BankPlus.Data.Models
{
    public partial class Account
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public bool? UpdateVirtualAccount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
