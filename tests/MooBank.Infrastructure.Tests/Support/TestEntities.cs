#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Bogus;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTagSettings = Asm.MooBank.Domain.Entities.Tag.TagSettings;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Infrastructure.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainUser CreateUser(
        Guid? id = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string currency = "AUD",
        Guid? familyId = null)
    {
        return new DomainUser(id ?? Guid.NewGuid())
        {
            EmailAddress = email ?? Faker.Internet.Email(),
            FirstName = firstName ?? Faker.Name.FirstName(),
            LastName = lastName ?? Faker.Name.LastName(),
            Currency = currency,
            FamilyId = familyId ?? Guid.NewGuid(),
        };
    }

    public static Models.User CreateUserModel(
        Guid? id = null,
        string? email = null,
        string currency = "AUD",
        Guid? familyId = null)
    {
        return new Models.User
        {
            Id = id ?? Guid.NewGuid(),
            EmailAddress = email ?? Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Currency = currency,
            FamilyId = familyId ?? Guid.NewGuid(),
            Accounts = [],
            SharedAccounts = [],
            Groups = [],
        };
    }

    public static Group CreateGroup(
        Guid? id = null,
        string? name = null,
        Guid? ownerId = null,
        bool showPosition = true)
    {
        return new Group(id ?? Guid.NewGuid())
        {
            Name = name ?? Faker.Commerce.Department(),
            OwnerId = ownerId ?? Guid.NewGuid(),
            ShowPosition = showPosition,
        };
    }

    public static Budget CreateBudget(
        Guid? id = null,
        Guid? familyId = null,
        short? year = null)
    {
        return new Budget(id ?? Guid.NewGuid())
        {
            FamilyId = familyId ?? Guid.NewGuid(),
            Year = year ?? (short)DateTime.Now.Year,
        };
    }

    public static BudgetLine CreateBudgetLine(
        Guid? id = null,
        Guid? budgetId = null,
        int tagId = 1,
        decimal amount = 100m)
    {
        return new BudgetLine(id ?? Guid.NewGuid())
        {
            BudgetId = budgetId ?? Guid.NewGuid(),
            TagId = tagId,
            Amount = amount,
        };
    }

    public static DomainTag CreateTag(
        int id = 0,
        string? name = null,
        Guid? familyId = null,
        bool deleted = false)
    {
        var tag = new DomainTag(id)
        {
            Name = name ?? Faker.Commerce.Department(),
            FamilyId = familyId ?? Guid.NewGuid(),
            Deleted = deleted,
        };
        return tag;
    }

    public static DomainTagSettings CreateTagSettings(
        int tagId,
        bool excludeFromReporting = false,
        bool applySmoothing = false)
    {
        return new DomainTagSettings(tagId)
        {
            ExcludeFromReporting = excludeFromReporting,
            ApplySmoothing = applySmoothing,
        };
    }

    public static LogicalAccount CreateLogicalAccount(
        Guid? id = null,
        string? name = null,
        string currency = "AUD",
        decimal balance = 0m,
        AccountType accountType = AccountType.Transaction,
        bool shareWithFamily = false)
    {
        return new LogicalAccount(id ?? Guid.NewGuid(), [])
        {
            Name = name ?? Faker.Finance.AccountName(),
            Currency = currency,
            Balance = balance,
            AccountType = accountType,
            ShareWithFamily = shareWithFamily,
        };
    }

    public static InstrumentOwner CreateInstrumentOwner(
        Guid instrumentId,
        Guid userId,
        Guid? groupId = null)
    {
        return new InstrumentOwner
        {
            InstrumentId = instrumentId,
            UserId = userId,
            GroupId = groupId,
        };
    }

    public static ImporterType CreateImporterType(
        int id = 1,
        string? name = null,
        string? typeName = "Asm.MooBank.Importers.TestImporter, Asm.MooBank")
    {
        return new ImporterType
        {
            ImporterTypeId = id,
            Name = name ?? "Test Importer",
            Type = typeName!,
        };
    }

    public static InstitutionAccount CreateInstitutionAccount(
        Guid? id = null,
        Guid? instrumentId = null,
        int? importerTypeId = 1,
        ImporterType? importerType = null)
    {
        var account = new InstitutionAccount(id ?? Guid.NewGuid())
        {
            InstrumentId = instrumentId ?? Guid.NewGuid(),
            ImporterTypeId = importerTypeId,
            Name = "Test Account",
            InstitutionId = 1,
            OpenedDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
        };

        if (importerType != null)
        {
            account.ImporterType = importerType;
        }

        return account;
    }

    public static StockPriceHistory CreateStockPriceHistory(
        string symbol = "TEST",
        string? exchange = null,
        DateOnly? date = null,
        decimal price = 100m)
    {
        return new StockPriceHistory
        {
            Symbol = symbol,
            Exchange = exchange,
            Date = date ?? DateOnly.FromDateTime(DateTime.Today),
            Price = price,
        };
    }
}
