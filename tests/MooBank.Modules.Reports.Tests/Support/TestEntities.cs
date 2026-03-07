#nullable enable
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using Bogus;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Reports.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static ReportType CreateDebitReportType()
    {
        ReportType.TryParse("debit", out var reportType);
        return reportType;
    }

    public static ReportType CreateCreditReportType()
    {
        ReportType.TryParse("credit", out var reportType);
        return reportType;
    }

    public static CreditDebitTotal CreateCreditDebitTotal(
        TransactionFilterType transactionType = TransactionFilterType.Credit,
        decimal total = 1000m)
    {
        return new CreditDebitTotal
        {
            TransactionType = transactionType,
            Total = total,
        };
    }

    public static CreditDebitAverage CreateCreditDebitAverage(
        TransactionFilterType transactionType = TransactionFilterType.Credit,
        decimal average = 500m)
    {
        return new CreditDebitAverage
        {
            TransactionType = transactionType,
            Average = average,
        };
    }

    public static TransactionTagTotal CreateTransactionTagTotal(
        int tagId = 1,
        string? tagName = null,
        decimal grossAmount = 100m,
        decimal netAmount = 90m,
        bool hasChildren = false)
    {
        return new TransactionTagTotal
        {
            TagId = tagId,
            TagName = tagName ?? Faker.Commerce.Department(),
            GrossAmount = grossAmount,
            NetAmount = netAmount,
            HasChildren = hasChildren,
        };
    }

    public static MonthlyTagTotal CreateMonthlyTagTotal(
        DateOnly? month = null,
        decimal grossAmount = 100m,
        decimal netAmount = 90m)
    {
        return new MonthlyTagTotal
        {
            Month = month ?? DateOnly.FromDateTime(DateTime.Today),
            GrossAmount = grossAmount,
            NetAmount = netAmount,
        };
    }

    public static MonthlyBalance CreateMonthlyBalance(
        DateOnly? periodEnd = null,
        decimal balance = 5000m)
    {
        return new MonthlyBalance
        {
            PeriodEnd = periodEnd ?? DateOnly.FromDateTime(DateTime.Today),
            Balance = balance,
        };
    }

    public static TagAverage CreateTagAverage(
        int tagId = 1,
        string? name = null,
        decimal average = 150m)
    {
        return new TagAverage
        {
            TagId = tagId,
            Name = name ?? Faker.Commerce.Department(),
            Average = average,
        };
    }

    public static DomainTag CreateTag(
        int id = 1,
        string? name = null,
        Guid? familyId = null,
        bool deleted = false)
    {
        return new DomainTag(id)
        {
            Name = name ?? Faker.Commerce.Department(),
            FamilyId = familyId ?? Guid.NewGuid(),
            Deleted = deleted,
        };
    }

    public static IQueryable<DomainTag> CreateTagQueryable(IEnumerable<DomainTag> tags)
    {
        return QueryableHelper.CreateAsyncQueryable(tags);
    }

    public static IQueryable<DomainTag> CreateTagQueryable(params DomainTag[] tags)
    {
        return CreateTagQueryable(tags.AsEnumerable());
    }

    public static List<CreditDebitTotal> CreateSampleCreditDebitTotals()
    {
        return
        [
            CreateCreditDebitTotal(TransactionFilterType.Credit, 5000m),
            CreateCreditDebitTotal(TransactionFilterType.Debit, 3500m),
        ];
    }

    public static List<CreditDebitAverage> CreateSampleCreditDebitAverages()
    {
        return
        [
            CreateCreditDebitAverage(TransactionFilterType.Credit, 2500m),
            CreateCreditDebitAverage(TransactionFilterType.Debit, 1750m),
        ];
    }

    public static List<MonthlyBalance> CreateSampleMonthlyBalances()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return
        [
            CreateMonthlyBalance(today.AddMonths(-2), 4000m),
            CreateMonthlyBalance(today.AddMonths(-1), 4500m),
            CreateMonthlyBalance(today, 5000m),
        ];
    }

    public static List<TransactionTagTotal> CreateSampleTransactionTagTotals()
    {
        return
        [
            CreateTransactionTagTotal(1, "Groceries", 500m, 450m),
            CreateTransactionTagTotal(2, "Utilities", 200m, 180m),
            CreateTransactionTagTotal(3, "Entertainment", 150m, 140m),
        ];
    }

    public static List<TagAverage> CreateSampleTagAverages()
    {
        return
        [
            CreateTagAverage(1, "Groceries", 250m),
            CreateTagAverage(2, "Utilities", 100m),
            CreateTagAverage(3, "Entertainment", 75m),
        ];
    }

    public static List<MonthlyTagTotal> CreateSampleMonthlyTagTotals()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return
        [
            CreateMonthlyTagTotal(today.AddMonths(-2), 100m, 90m),
            CreateMonthlyTagTotal(today.AddMonths(-1), 120m, 110m),
            CreateMonthlyTagTotal(today, 150m, 140m),
        ];
    }
}
