using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Web.Api.Models
{
    public class TransactionsModel
    {
        public IEnumerable<Transaction> Transactions { get; set; }

        public int Total { get; set; }
        public int? PageNumber { get; internal set; }
    }
}
