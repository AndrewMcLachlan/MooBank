using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Bogus;

namespace Asm.MooBank.Modules.Tests;
internal class Models
{
    public static readonly Guid InstitutionId = new("841abac9-db4e-4a7a-81d1-561b04c2f5c4");
    public static readonly Guid AccountId = new("f1b1b1b1-1b1b-1b1b-1b1b-1b1b1b1b1b1b");
    public static readonly Guid GroupId = new("243e7609-8868-4e0e-996d-31d16ddbe220");
    public static readonly Guid FamilyId = new("cb0a08af-2054-4873-9add-c259916d2d43");
    public static readonly Guid UserId = new("35462a0c-d902-41cb-bbee-de7acb943739");

    public static readonly Guid InvalidAccountId = new("35462a0c-d902-41cb-bbee-de7acb943739");
    public static readonly Guid InvalidGroupId = new("35462a0c-d902-41cb-bbee-de7acb943739");
    public static readonly Guid InvalidAccountFamilyId = new("35462a0c-d902-41cb-bbee-de7acb943739");


    public readonly LogicalAccount Account = new()
    {
        Controller = Controller.Manual,
        Currency = "AUD",
        CurrentBalance = 1000,
        CurrentBalanceLocalCurrency = 1000,
        Id = AccountId,
        Name = "Test Account",
        GroupId = GroupId,
        AccountType = AccountType.Transaction,
        InstrumentType = AccountType.Transaction.ToString(),
        BalanceDate = DateTime.UtcNow,
        Description = "Test Account Description",
        InstitutionAccounts =
        [
            new()
            {
                InstitutionId = 1,
                ImporterTypeId =1 ,
            }
        ],
    };

    public static readonly User AccountHolder = new()
    {
        Id = UserId,
        EmailAddress = "mock@mclachlan.family",
        FamilyId = FamilyId,
        Currency = "AUD",
    };
}
