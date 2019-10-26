using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Web.Models.NewAccount;

namespace Asm.BankPlus.Web.Models
{
    public partial class NewAccountModel
    {

        public Account Account { get; set; }

        public ImportAccount ImportAccount { get; set; }
    }
}
