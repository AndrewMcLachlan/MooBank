using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.BankPlus.Data.Entities
{
    public partial class ImporterType
    {
        public int ImporterTypeId { get; set; }

        public string Type { get; set; }

        //public virtual ICollection<ImportAccount> ImportAccounts { get; set; }
    }
}
