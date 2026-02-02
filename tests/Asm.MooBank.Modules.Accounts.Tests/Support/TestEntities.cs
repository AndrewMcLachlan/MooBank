#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Bogus;
using ModelLogicalAccount = Asm.MooBank.Modules.Accounts.Models.Account.LogicalAccount;

namespace Asm.MooBank.Modules.Accounts.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static LogicalAccount CreateLogicalAccount(
        Guid? id = null,
        string? name = null,
        AccountType accountType = AccountType.Transaction,
        Controller controller = Controller.Manual,
        string currency = "AUD",
        decimal balance = 1000m,
        bool includeInBudget = true,
        bool shareWithFamily = false,
        IEnumerable<InstitutionAccount>? institutionAccounts = null,
        IEnumerable<InstrumentOwner>? owners = null)
    {
        var accountId = id ?? Guid.NewGuid();
        var instAccounts = institutionAccounts?.ToList() ?? [];

        var account = new LogicalAccount(accountId, instAccounts)
        {
            Name = name ?? Faker.Finance.AccountName(),
            AccountType = accountType,
            Controller = controller,
            Currency = currency,
            Balance = balance,
            IncludeInBudget = includeInBudget,
            ShareWithFamily = shareWithFamily,
        };

        if (owners != null)
        {
            foreach (var owner in owners)
            {
                account.Owners.Add(owner);
            }
        }

        return account;
    }

    public static InstitutionAccount CreateInstitutionAccount(
        Guid? id = null,
        Guid? instrumentId = null,
        string? name = null,
        int institutionId = 1,
        int? importerTypeId = null,
        DateOnly? openedDate = null,
        DateOnly? closedDate = null)
    {
        return new InstitutionAccount(id ?? Guid.NewGuid())
        {
            InstrumentId = instrumentId ?? Guid.NewGuid(),
            Name = name ?? Faker.Finance.AccountName(),
            InstitutionId = institutionId,
            ImporterTypeId = importerTypeId,
            OpenedDate = openedDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ClosedDate = closedDate,
        };
    }

    public static InstrumentOwner CreateInstrumentOwner(
        Guid? userId = null,
        Guid? groupId = null)
    {
        return new InstrumentOwner
        {
            UserId = userId ?? Guid.NewGuid(),
            GroupId = groupId,
        };
    }

    public static List<LogicalAccount> CreateSampleLogicalAccounts(Guid? ownerId = null)
    {
        var owner = ownerId ?? Guid.NewGuid();

        return
        [
            CreateLogicalAccount(
                name: "Everyday Account",
                accountType: AccountType.Transaction,
                balance: 2500m,
                owners: [CreateInstrumentOwner(owner)]),
            CreateLogicalAccount(
                name: "Savings Account",
                accountType: AccountType.Savings,
                balance: 15000m,
                owners: [CreateInstrumentOwner(owner)]),
            CreateLogicalAccount(
                name: "Credit Card",
                accountType: AccountType.Credit,
                balance: -500m,
                owners: [CreateInstrumentOwner(owner)]),
            CreateLogicalAccount(
                name: "Super Fund",
                accountType: AccountType.Superannuation,
                balance: 150000m,
                owners: [CreateInstrumentOwner(owner)]),
        ];
    }

    public static IQueryable<LogicalAccount> CreateLogicalAccountQueryable(IEnumerable<LogicalAccount> accounts)
    {
        return QueryableHelper.CreateAsyncQueryable(accounts);
    }

    public static IQueryable<LogicalAccount> CreateLogicalAccountQueryable(params LogicalAccount[] accounts)
    {
        return CreateLogicalAccountQueryable(accounts.AsEnumerable());
    }

    public static ModelLogicalAccount CreateLogicalAccountModel(
        Guid? id = null,
        string name = "Test Account",
        string? description = null,
        AccountType accountType = AccountType.Transaction,
        Controller controller = Controller.Manual,
        string currency = "AUD",
        decimal currentBalance = 1000m,
        bool includeInBudget = true,
        bool shareWithFamily = false,
        Guid? groupId = null)
    {
        return new ModelLogicalAccount
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            AccountType = accountType,
            Controller = controller,
            Currency = currency,
            CurrentBalance = currentBalance,
            CurrentBalanceLocalCurrency = currentBalance,
            IncludeInBudget = includeInBudget,
            ShareWithFamily = shareWithFamily,
            GroupId = groupId,
        };
    }
}
