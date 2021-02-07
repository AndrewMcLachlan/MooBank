using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Tools.TransactionGenerator
{
    public class Transaction : ICloneable
    {
        public DateTime Date { get; set; }

        public string Description { get; set; }

        public decimal Credit { get; set; }

        public decimal Debit { get; set; }

        public decimal Balance { get; set; }

        public int Frequency { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
