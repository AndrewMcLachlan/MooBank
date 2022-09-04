using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Web.Models
{
    public class TransactionTagRuleModel
    {
        public string Contains { get; set; }

        public IEnumerable<TransactionTag> Tags { get; set; }
    }
}
