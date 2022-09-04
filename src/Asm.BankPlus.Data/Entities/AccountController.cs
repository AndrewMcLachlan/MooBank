using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Data.Entities
{
    public class AccountController
    {
        public int AccountControllerId { get; set; }

        [Column("AccountcontrollerType")]
        public string ControllerType { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
