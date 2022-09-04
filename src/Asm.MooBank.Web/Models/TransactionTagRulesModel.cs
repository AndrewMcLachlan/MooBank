using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Web.Models
{
    public class TransactionTagRulesModel
    {
        public IEnumerable<TransactionTagRule> Rules { get; set; }
    }
}
