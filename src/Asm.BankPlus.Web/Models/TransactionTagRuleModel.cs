using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Web.Models
{
    public class TransactionTagRuleModel
    {
        public string Contains { get; set; }

        public IEnumerable<int> Tags { get; set; }
    }
}
