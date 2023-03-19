using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities;

public class __AccountController
{
    public int AccountControllerId { get; set; }

    [Column("AccountcontrollerType")]
    public string ControllerType { get; set; }

    public virtual ICollection<Account.Account> Accounts { get; set; }
}
