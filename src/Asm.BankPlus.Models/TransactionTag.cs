using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus.Models
{
    public class TransactionTag
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<TransactionTag> Tags { get; set; }
    }
}
