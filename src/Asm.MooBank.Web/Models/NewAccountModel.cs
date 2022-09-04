using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.MooBank.Web.Models.NewAccount;

namespace Asm.MooBank.Web.Models
{
    public partial class NewAccountModel
    {

        public MooBank.Models.Account Account { get; set; }

        public ImportAccount ImportAccount { get; set; }
    }
}
