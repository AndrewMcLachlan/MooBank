using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asm.BankPlus.Web.Models
{
    public class UpdateBalanceModel
    {
        public decimal? CurrentBalance { get; set; }

        public decimal? AvailableBalance { get; set; }
    }
}
