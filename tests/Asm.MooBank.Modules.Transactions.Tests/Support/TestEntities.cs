#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Bogus;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;

namespace Asm.MooBank.Modules.Transactions.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static TestTransactionInstrument CreateTransactionInstrument(
        Guid? id = null,
        string name = "Test Account",
        decimal balance = 1000m,
        string currency = "AUD")
    {
        var instrumentId = id ?? Guid.NewGuid();
        return new TestTransactionInstrument(instrumentId)
        {
            Name = name,
            Balance = balance,
            Currency = currency,
            Controller = Controller.Manual,
        };
    }

    public static DomainTransaction CreateTransaction(
        Guid? id = null,
        Guid? accountId = null,
        Guid? accountHolderId = null,
        decimal amount = -50m,
        string? description = null,
        DateTime? transactionTime = null,
        TransactionType? transactionType = null,
        string? reference = null,
        string? notes = null,
        bool excludeFromReporting = false,
        IEnumerable<DomainTransactionSplit>? splits = null,
        IEnumerable<DomainTag>? tags = null)
    {
        var transactionId = id ?? Guid.NewGuid();
        var type = transactionType ?? (amount < 0 ? TransactionType.Debit : TransactionType.Credit);

        var transaction = new DomainTransaction(transactionId)
        {
            AccountId = accountId ?? Guid.NewGuid(),
            AccountHolderId = accountHolderId,
            Amount = amount,
            Description = description ?? Faker.Commerce.ProductName(),
            TransactionTime = transactionTime ?? DateTime.UtcNow,
            TransactionType = type,
            Reference = reference,
            Notes = notes,
            ExcludeFromReporting = excludeFromReporting,
            Source = "Test",
        };

        // Add splits
        if (splits != null)
        {
            foreach (var split in splits)
            {
                split.TransactionId = transactionId;
                AddSplitToTransaction(transaction, split);
            }
        }
        else
        {
            // Create default split
            var defaultSplit = CreateTransactionSplit(transactionId: transactionId, amount: Math.Abs(amount), tags: tags);
            AddSplitToTransaction(transaction, defaultSplit);
        }

        return transaction;
    }

    public static DomainTransactionSplit CreateTransactionSplit(
        Guid? id = null,
        Guid? transactionId = null,
        decimal amount = 50m,
        IEnumerable<DomainTag>? tags = null)
    {
        var split = new DomainTransactionSplit(id ?? Guid.NewGuid())
        {
            TransactionId = transactionId ?? Guid.NewGuid(),
            Amount = amount,
        };

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                split.Tags.Add(tag);
            }
        }

        return split;
    }

    public static DomainTag CreateTag(int id = 1, string name = "Test Tag", Guid? familyId = null)
    {
        return new DomainTag(id)
        {
            Name = name,
            FamilyId = familyId ?? Guid.NewGuid(),
        };
    }

    public static MooBank.Models.TransactionSplit CreateTransactionSplitModel(
        Guid? id = null,
        decimal amount = 50m,
        IEnumerable<MooBank.Models.Tag>? tags = null)
    {
        return new MooBank.Models.TransactionSplit
        {
            Id = id ?? Guid.NewGuid(),
            Amount = amount,
            Tags = tags ?? [],
            OffsetBy = [],
        };
    }

    public static MooBank.Models.Tag CreateTagModel(int id = 1, string name = "Test Tag")
    {
        return new MooBank.Models.Tag
        {
            Id = id,
            Name = name,
        };
    }

    public static IQueryable<DomainTransaction> CreateTransactionQueryable(IEnumerable<DomainTransaction> transactions)
    {
        return QueryableHelper.CreateAsyncQueryable(transactions);
    }

    public static IQueryable<DomainTransaction> CreateTransactionQueryable(params DomainTransaction[] transactions)
    {
        return CreateTransactionQueryable(transactions.AsEnumerable());
    }

    // Helper method to add split to transaction using reflection (since _splits is private)
    private static void AddSplitToTransaction(DomainTransaction transaction, DomainTransactionSplit split)
    {
        var splitsField = typeof(DomainTransaction).GetField("_splits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var splits = splitsField?.GetValue(transaction) as List<DomainTransactionSplit>;
        splits?.Add(split);
    }
}

// Concrete implementation of TransactionInstrument for testing
public class TestTransactionInstrument : DomainInstrument.TransactionInstrument
{
    public TestTransactionInstrument(Guid id) : base(id)
    {
    }
}
