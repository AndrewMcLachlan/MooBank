using Asm.MooBank.Web.Models.NewAccount;

namespace Asm.MooBank.Web.Models;

public partial class NewAccountModel
{

    public MooBank.Models.InstitutionAccount Account { get; set; }

    public ImportAccount ImportAccount { get; set; }
}
