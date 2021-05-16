using System;

namespace Asm.BankPlus.Models
{
    public class VirtualAccount
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
    }
}
