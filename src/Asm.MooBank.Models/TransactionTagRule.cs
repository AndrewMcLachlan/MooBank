using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.MooBank.Models
{
    public class TransactionTagRule
    {
        public int Id { get; set; }

        public string Contains { get; set; }

        public IEnumerable<TransactionTag> Tags { get; set; }
    }
}
