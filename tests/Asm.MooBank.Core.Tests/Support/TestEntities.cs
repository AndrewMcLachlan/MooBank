using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Bogus;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using TransactionOffset = Asm.MooBank.Domain.Entities.Transactions.TransactionOffset;
using DomainVirtualInstrument = Asm.MooBank.Domain.Entities.Account.VirtualInstrument;

namespace Asm.MooBank.Core.Tests.Support;

public class TestEntities
{
    public readonly Faker Faker = new();

    // Users
    public MooBank.Domain.Entities.User.User CreateUser(Guid id = default, Guid familyId = default) =>
        new(id == default ? Guid.NewGuid() : id)
        {
            EmailAddress = Faker.Internet.Email(),
            FamilyId = familyId == default ? TestModels.FamilyId : familyId,
        };

    public MooBank.Domain.Entities.User.User Owner { get; }
    public MooBank.Domain.Entities.User.User FamilyUser { get; }
    public MooBank.Domain.Entities.User.User OtherUser { get; }

    // Account
    public LogicalAccount Account { get; }

    public TestEntities()
    {
        Owner = new MooBank.Domain.Entities.User.User(TestModels.UserId)
        {
            EmailAddress = "owner@mclachlan.family",
            FamilyId = TestModels.FamilyId,
        };

        FamilyUser = new MooBank.Domain.Entities.User.User(new Guid("5a0cda81-3ab6-43d3-85e9-fa0e323881ff"))
        {
            EmailAddress = "family@mclachlan.family",
            FamilyId = TestModels.FamilyId,
        };

        OtherUser = new MooBank.Domain.Entities.User.User(new Guid("88888888-8888-8888-8888-888888888888"))
        {
            EmailAddress = "other@example.com",
            FamilyId = TestModels.OtherFamilyId,
        };

        Account = new LogicalAccount(TestModels.AccountId, [])
        {
            Controller = Controller.Manual,
            Currency = "AUD",
            Balance = 1000,
            Name = "Test Account",
            AccountType = AccountType.Transaction,
            LastTransaction = DateOnly.FromDateTime(DateTime.Today),
            Description = "Test Account Description",
            Owners =
            [
                new InstrumentOwner
                {
                    UserId = TestModels.UserId,
                    User = Owner,
                }
            ]
        };
    }

    // Transaction Factories
    public Transaction CreateTransaction(
        decimal amount = -50m,
        string? description = null,
        DateTime transactionTime = default,
        Guid accountId = default)
    {
        return Transaction.Create(
            accountId == default ? TestModels.AccountId : accountId,
            TestModels.UserId,
            amount,
            description ?? Faker.Commerce.ProductName(),
            transactionTime == default ? DateTime.UtcNow : transactionTime,
            null,
            "Test",
            null);
    }

    public Transaction CreateDebitTransaction(decimal amount = 50m, string? description = null) =>
        CreateTransaction(-Math.Abs(amount), description);

    public Transaction CreateCreditTransaction(decimal amount = 50m, string? description = null) =>
        CreateTransaction(Math.Abs(amount), description);

    // Tag Factory
    public Asm.MooBank.Domain.Entities.Tag.Tag CreateTag(int id = 0, string? name = null) =>
        new(id == 0 ? Faker.Random.Int(1, 1000) : id)
        {
            Name = name ?? Faker.Commerce.Categories(1)[0],
            FamilyId = TestModels.FamilyId,
        };

    // TransactionOffset Factory
    public TransactionOffset CreateOffset(Guid splitId, Guid offsetTransactionId, decimal amount = 10m) =>
        new()
        {
            TransactionSplitId = splitId,
            OffsetTransactionId = offsetTransactionId,
            Amount = amount,
        };

    // ForecastPlan Factory
    public ForecastPlan CreateForecastPlan(Guid id = default, string? name = null) =>
        new(id == default ? TestModels.ForecastPlanId : id)
        {
            FamilyId = TestModels.FamilyId,
            Name = name ?? "Test Forecast",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };

    // ForecastPlannedItem Factory
    public ForecastPlannedItem CreatePlannedItem(Guid id = default, string? name = null, decimal amount = 100m) =>
        new(id == default ? Guid.NewGuid() : id)
        {
            Name = name ?? "Test Item",
            Amount = amount,
            ItemType = PlannedItemType.Expense,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
        };

    // VirtualInstrument Factory
    public DomainVirtualInstrument CreateVirtualInstrument(Guid id = default, string? name = null, Guid parentId = default) =>
        new(id == default ? TestModels.VirtualInstrumentId : id)
        {
            Name = name ?? "Virtual Account",
            Currency = "AUD",
            ParentInstrumentId = parentId == default ? TestModels.AccountId : parentId,
        };
}
