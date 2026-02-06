#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Models;
using Bogus;
using DomainBudget = Asm.MooBank.Domain.Entities.Budget.Budget;
using DomainBudgetLine = Asm.MooBank.Domain.Entities.Budget.BudgetLine;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;
using ModelBudgetLine = Asm.MooBank.Modules.Budgets.Models.BudgetLine;
using ModelBudgetLineType = Asm.MooBank.Modules.Budgets.Models.BudgetLineType;

namespace Asm.MooBank.Modules.Budgets.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainBudget CreateBudget(
        Guid? id = null,
        short year = 2024,
        Guid? familyId = null,
        IEnumerable<DomainBudgetLine>? lines = null)
    {
        var budgetId = id ?? Guid.NewGuid();
        var budget = new DomainBudget(budgetId)
        {
            Year = year,
            FamilyId = familyId ?? Guid.NewGuid(),
        };

        if (lines != null)
        {
            foreach (var line in lines)
            {
                line.BudgetId = budgetId;
                budget.Lines.Add(line);
            }
        }

        return budget;
    }

    public static DomainBudgetLine CreateBudgetLine(
        Guid? id = null,
        Guid? budgetId = null,
        int tagId = 1,
        string tagName = "Test Tag",
        string? notes = null,
        decimal amount = 100m,
        bool income = false,
        short month = 4095)
    {
        var lineId = id ?? Guid.NewGuid();
        return new DomainBudgetLine(lineId)
        {
            BudgetId = budgetId ?? Guid.NewGuid(),
            TagId = tagId,
            Tag = CreateTag(tagId, tagName),
            Notes = notes,
            Amount = amount,
            Income = income,
            Month = month,
        };
    }

    public static ModelBudgetLine CreateBudgetLineModel(
        Guid? id = null,
        int tagId = 1,
        string name = "Test Tag",
        string? notes = null,
        decimal amount = 100m,
        short month = 4095,
        ModelBudgetLineType type = ModelBudgetLineType.Expenses)
    {
        return new ModelBudgetLine
        {
            Id = id ?? Guid.NewGuid(),
            TagId = tagId,
            Name = name,
            Notes = notes,
            Amount = amount,
            Month = month,
            Type = type,
        };
    }

    public static IQueryable<DomainBudget> CreateBudgetQueryable(IEnumerable<DomainBudget> budgets)
    {
        return QueryableHelper.CreateAsyncQueryable(budgets);
    }

    public static IQueryable<DomainBudget> CreateBudgetQueryable(params DomainBudget[] budgets)
    {
        return CreateBudgetQueryable(budgets.AsEnumerable());
    }

    public static IQueryable<DomainBudgetLine> CreateBudgetLineQueryable(IEnumerable<DomainBudgetLine> lines)
    {
        return QueryableHelper.CreateAsyncQueryable(lines);
    }

    public static IQueryable<DomainBudgetLine> CreateBudgetLineQueryable(params DomainBudgetLine[] lines)
    {
        return CreateBudgetLineQueryable(lines.AsEnumerable());
    }

    public static DomainTag CreateTag(
        int id = 1,
        string name = "Test Tag",
        Guid? familyId = null,
        TagSettings? settings = null)
    {
        return new DomainTag(id)
        {
            Name = name,
            FamilyId = familyId ?? Guid.NewGuid(),
            Settings = settings ?? new TagSettings(id),
        };
    }

    public static DomainTransaction CreateTransaction(
        Guid? id = null,
        Guid? accountId = null,
        decimal amount = -100m,
        DateTime? transactionTime = null,
        TransactionType? transactionType = null,
        bool excludeFromReporting = false,
        IEnumerable<DomainTransactionSplit>? splits = null)
    {
        var txnId = id ?? Guid.NewGuid();
        var txnType = transactionType ?? (amount < 0 ? TransactionType.Debit : TransactionType.Credit);

        var transaction = new DomainTransaction(txnId)
        {
            AccountId = accountId ?? Guid.NewGuid(),
            Amount = amount,
            TransactionTime = transactionTime ?? DateTime.UtcNow,
            TransactionType = txnType,
            ExcludeFromReporting = excludeFromReporting,
            Source = "Test",
        };

        if (splits != null)
        {
            foreach (var split in splits)
            {
                split.TransactionId = txnId;
                var splitsField = typeof(DomainTransaction).GetField("_splits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var splitsList = splitsField?.GetValue(transaction) as List<DomainTransactionSplit>;
                splitsList?.Add(split);
            }
        }
        else
        {
            // Add default split
            var defaultSplit = new DomainTransactionSplit(Guid.NewGuid())
            {
                TransactionId = txnId,
                Amount = Math.Abs(amount),
            };
            var splitsField = typeof(DomainTransaction).GetField("_splits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var splitsList = splitsField?.GetValue(transaction) as List<DomainTransactionSplit>;
            splitsList?.Add(defaultSplit);
        }

        return transaction;
    }

    public static DomainTransactionSplit CreateTransactionSplit(
        Guid? id = null,
        Guid? transactionId = null,
        decimal amount = 100m,
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

    public static LogicalAccount CreateLogicalAccount(
        Guid? id = null,
        string? name = null,
        bool includeInBudget = true)
    {
        var accountId = id ?? Guid.NewGuid();
        return new LogicalAccount(accountId, [])
        {
            Name = name ?? Faker.Company.CompanyName(),
            IncludeInBudget = includeInBudget,
        };
    }

    public static TagRelationship CreateTagRelationship(
        int tagId,
        int parentTagId,
        long ordinal = 0)
    {
        return new TagRelationship
        {
            Id = tagId,
            ParentId = parentTagId,
            Ordinal = ordinal,
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

    public static IQueryable<LogicalAccount> CreateLogicalAccountQueryable(IEnumerable<LogicalAccount> accounts)
    {
        return QueryableHelper.CreateAsyncQueryable(accounts);
    }

    public static IQueryable<LogicalAccount> CreateLogicalAccountQueryable(params LogicalAccount[] accounts)
    {
        return CreateLogicalAccountQueryable(accounts.AsEnumerable());
    }

    public static IQueryable<TagRelationship> CreateTagRelationshipQueryable(IEnumerable<TagRelationship> relationships)
    {
        return QueryableHelper.CreateAsyncQueryable(relationships);
    }

    public static IQueryable<TagRelationship> CreateTagRelationshipQueryable(params TagRelationship[] relationships)
    {
        return CreateTagRelationshipQueryable(relationships.AsEnumerable());
    }
}
