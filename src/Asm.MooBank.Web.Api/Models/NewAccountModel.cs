using Asm.MooBank.Modules.Account.Models.Account;

namespace Asm.MooBank.Web.Api.Models;

public partial record NewAccountModel
{

    public required InstitutionAccount Account { get; init; }

    public ImportAccount? ImportAccount { get; init; }
}
