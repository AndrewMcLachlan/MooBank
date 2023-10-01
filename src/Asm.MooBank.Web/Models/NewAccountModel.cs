using Asm.MooBank.Web.Models.NewAccount;

namespace Asm.MooBank.Web.Models;

public partial record NewAccountModel
{

    public required InstitutionAccount Account { get; init; }

    public ImportAccount? ImportAccount { get; init; }
}
