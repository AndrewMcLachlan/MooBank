#nullable enable
using Asm.MooBank.Domain.Entities.Utility;
using Asm.MooBank.Models;
using Bogus;
using DomainAccount = Asm.MooBank.Domain.Entities.Utility.Account;

namespace Asm.MooBank.Modules.Bills.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainAccount CreateAccount(
        Guid? id = null,
        string? name = null,
        string? description = null,
        UtilityType utilityType = UtilityType.Electricity,
        string accountNumber = "12345678",
        int? institutionId = null,
        string currency = "AUD",
        bool shareWithFamily = true,
        IEnumerable<Bill>? bills = null)
    {
        var accountId = id ?? Guid.NewGuid();

        var account = new DomainAccount(accountId)
        {
            Name = name ?? Faker.Company.CompanyName() + " " + utilityType,
            Description = description,
            UtilityType = utilityType,
            AccountNumber = accountNumber,
            InstitutionId = institutionId,
            Currency = currency,
            Controller = Controller.Manual,
            ShareWithFamily = shareWithFamily,
        };

        if (bills != null)
        {
            foreach (var bill in bills)
            {
                bill.AccountId = accountId;
                bill.Account = account; // Set navigation property
                account.Bills.Add(bill);
            }
        }

        return account;
    }

    public static Bill CreateBill(
        int id = 0,
        Guid? accountId = null,
        string? invoiceNumber = null,
        DateOnly? issueDate = null,
        int? currentReading = null,
        int? previousReading = null,
        decimal? cost = null,
        bool? costsIncludeGST = true,
        IEnumerable<Period>? periods = null,
        IEnumerable<Discount>? discounts = null)
    {
        var bill = new Bill
        {
            Id = id,
            AccountId = accountId ?? Guid.NewGuid(),
            InvoiceNumber = invoiceNumber ?? Faker.Random.AlphaNumeric(10),
            IssueDate = issueDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CurrentReading = currentReading,
            PreviousReading = previousReading,
            Cost = cost ?? Faker.Random.Decimal(50, 500),
            CostsIncludeGST = costsIncludeGST,
        };

        if (periods != null)
        {
            foreach (var period in periods)
            {
                bill.Periods.Add(period);
            }
        }

        if (discounts != null)
        {
            foreach (var discount in discounts)
            {
                bill.Discounts.Add(discount);
            }
        }

        return bill;
    }

    public static Period CreatePeriod(
        int id = 0,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        decimal? chargePerDay = null,
        decimal? pricePerUnit = null,
        int? totalUsage = null)
    {
        var start = periodStart ?? DateTime.UtcNow.AddMonths(-1);
        var end = periodEnd ?? DateTime.UtcNow;

        return new Period
        {
            Id = id,
            PeriodStart = start,
            PeriodEnd = end,
            ServiceCharge = new ServiceCharge
            {
                ChargePerDay = chargePerDay ?? Faker.Random.Decimal(0.5m, 2m),
            },
            Usage = new Usage
            {
                PricePerUnit = pricePerUnit ?? Faker.Random.Decimal(0.1m, 0.5m),
                TotalUsage = totalUsage ?? Faker.Random.Int(100, 1000),
            },
        };
    }

    public static Discount CreateDiscount(
        int id = 0,
        byte? discountPercent = null,
        decimal? discountAmount = null,
        string? reason = null)
    {
        return new Discount
        {
            Id = id,
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            Reason = reason ?? "Early payment discount",
        };
    }

    public static List<DomainAccount> CreateSampleAccounts(Guid? ownerId = null)
    {
        return
        [
            CreateAccount(
                name: "AGL Electricity",
                utilityType: UtilityType.Electricity,
                accountNumber: "ELEC001"),
            CreateAccount(
                name: "Origin Gas",
                utilityType: UtilityType.Gas,
                accountNumber: "GAS001"),
            CreateAccount(
                name: "Sydney Water",
                utilityType: UtilityType.Water,
                accountNumber: "WATER001"),
            CreateAccount(
                name: "Telstra Internet",
                utilityType: UtilityType.Internet,
                accountNumber: "NET001"),
        ];
    }

    public static IQueryable<DomainAccount> CreateAccountQueryable(IEnumerable<DomainAccount> accounts)
    {
        return QueryableHelper.CreateAsyncQueryable(accounts);
    }

    public static IQueryable<DomainAccount> CreateAccountQueryable(params DomainAccount[] accounts)
    {
        return CreateAccountQueryable(accounts.AsEnumerable());
    }

    public static DomainAccount CreateAccountWithOwner(
        Guid? id = null,
        string? name = null,
        UtilityType utilityType = UtilityType.Electricity,
        Guid? ownerId = null,
        IEnumerable<Bill>? bills = null)
    {
        var accountId = id ?? Guid.NewGuid();
        var account = CreateAccount(
            id: accountId,
            name: name,
            utilityType: utilityType,
            bills: bills);

        var owner = new Domain.Entities.Instrument.InstrumentOwner
        {
            UserId = ownerId ?? Guid.NewGuid(),
            InstrumentId = accountId,
        };
        account.Owners.Add(owner);

        return account;
    }

    public static Models.ImportBill CreateImportBill(
        string accountName = "Test Account",
        DateOnly? issueDate = null,
        string? invoiceNumber = null,
        decimal cost = 100m,
        bool costsIncludeGST = true,
        int? currentReading = null,
        int? previousReading = null,
        IEnumerable<Models.Period>? periods = null,
        IEnumerable<Models.Discount>? discounts = null)
    {
        return new Models.ImportBill
        {
            AccountName = accountName,
            IssueDate = issueDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            InvoiceNumber = invoiceNumber ?? Faker.Random.AlphaNumeric(10),
            Cost = cost,
            CostsIncludeGST = costsIncludeGST,
            CurrentReading = currentReading,
            PreviousReading = previousReading,
            Periods = periods ?? [],
            Discounts = discounts ?? [],
        };
    }

    public static Models.Period CreateModelPeriod(
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        decimal chargePerDay = 1.0m,
        decimal pricePerUnit = 0.25m,
        int totalUsage = 500)
    {
        return new Models.Period
        {
            PeriodStart = periodStart ?? DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = periodEnd ?? DateTime.UtcNow,
            ChargePerDay = chargePerDay,
            PricePerUnit = pricePerUnit,
            TotalUsage = totalUsage,
        };
    }

    public static Models.Discount CreateModelDiscount(
        byte? discountPercent = null,
        decimal? discountAmount = null,
        string reason = "Discount")
    {
        return new Models.Discount
        {
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            Reason = reason,
        };
    }
}
