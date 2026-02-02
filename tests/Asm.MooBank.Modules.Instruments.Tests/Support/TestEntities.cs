#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Virtual;
using Bogus;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;
using DomainRule = Asm.MooBank.Domain.Entities.Instrument.Rule;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainVirtualInstrument = Asm.MooBank.Domain.Entities.Account.VirtualInstrument;

namespace Asm.MooBank.Modules.Instruments.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static LogicalAccount CreateInstrument(
        Guid? id = null,
        string? name = null,
        string? description = null,
        string currency = "AUD",
        AccountType accountType = AccountType.Transaction,
        IEnumerable<DomainRule>? rules = null,
        IEnumerable<DomainVirtualInstrument>? virtualInstruments = null)
    {
        var instrumentId = id ?? Guid.NewGuid();
        var instrument = new LogicalAccount(instrumentId, [])
        {
            Name = name ?? Faker.Finance.AccountName(),
            Description = description ?? Faker.Lorem.Sentence(),
            Currency = currency,
            AccountType = accountType,
        };

        if (rules != null)
        {
            foreach (var rule in rules)
            {
                rule.InstrumentId = instrumentId;
                instrument.Rules.Add(rule);
            }
        }

        if (virtualInstruments != null)
        {
            foreach (var vi in virtualInstruments)
            {
                vi.ParentInstrumentId = instrumentId;
                instrument.AddVirtualInstrument(vi, 0m);
            }
            // Clear events raised by AddVirtualInstrument so tests start clean
            instrument.Events.Clear();
        }

        return instrument;
    }

    public static DomainVirtualInstrument CreateVirtualInstrument(
        Guid? id = null,
        Guid? parentId = null,
        string? name = null,
        string? description = null,
        string currency = "AUD",
        decimal balance = 0m,
        Controller controller = Controller.Virtual)
    {
        return new DomainVirtualInstrument(id ?? Guid.NewGuid())
        {
            ParentInstrumentId = parentId ?? Guid.NewGuid(),
            Name = name ?? Faker.Finance.AccountName(),
            Description = description ?? Faker.Lorem.Sentence(),
            Currency = currency,
            Balance = balance,
            Controller = controller,
        };
    }

    public static CreateVirtualInstrument CreateVirtualInstrumentModel(
        string? name = null,
        string? description = null,
        decimal openingBalance = 0m,
        Controller controller = Controller.Virtual)
    {
        return new CreateVirtualInstrument
        {
            Name = name ?? Faker.Finance.AccountName(),
            Description = description ?? Faker.Lorem.Sentence(),
            OpeningBalance = openingBalance,
            Controller = controller,
        };
    }

    public static IQueryable<LogicalAccount> CreateLogicalAccountQueryable(IEnumerable<LogicalAccount> accounts)
    {
        return QueryableHelper.CreateAsyncQueryable(accounts);
    }

    public static IQueryable<LogicalAccount> CreateLogicalAccountQueryable(params LogicalAccount[] accounts)
    {
        return CreateLogicalAccountQueryable(accounts.AsEnumerable());
    }

    public static DomainRule CreateRule(
        int id = 1,
        Guid? instrumentId = null,
        string? contains = null,
        string? description = null,
        IEnumerable<DomainTag>? tags = null)
    {
        var rule = new DomainRule(id)
        {
            InstrumentId = instrumentId ?? Guid.NewGuid(),
            Contains = contains ?? Faker.Commerce.ProductName(),
            Description = description ?? Faker.Lorem.Sentence(),
        };

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                rule.Tags.Add(tag);
            }
        }

        return rule;
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

    public static MooBank.Models.Tag CreateTagModel(
        int id = 1,
        string? name = null)
    {
        return new MooBank.Models.Tag
        {
            Id = id,
            Name = name ?? Faker.Commerce.Department(),
            Tags = [],
        };
    }

    public static Models.Rules.UpdateRule CreateUpdateRule(
        string? contains = null,
        string? description = null,
        IEnumerable<MooBank.Models.Tag>? tags = null)
    {
        return new Models.Rules.UpdateRule
        {
            Contains = contains ?? Faker.Commerce.ProductName(),
            Description = description ?? Faker.Lorem.Sentence(),
            Tags = tags ?? [],
        };
    }

    public static List<DomainRule> CreateSampleRules(Guid? instrumentId = null)
    {
        var iid = instrumentId ?? Guid.NewGuid();
        return
        [
            CreateRule(id: 1, instrumentId: iid, contains: "WOOLWORTHS"),
            CreateRule(id: 2, instrumentId: iid, contains: "COLES"),
            CreateRule(id: 3, instrumentId: iid, contains: "NETFLIX"),
        ];
    }

    public static IQueryable<DomainInstrument> CreateInstrumentQueryable(IEnumerable<DomainInstrument> instruments)
    {
        return QueryableHelper.CreateAsyncQueryable(instruments);
    }

    public static IQueryable<DomainInstrument> CreateInstrumentQueryable(params DomainInstrument[] instruments)
    {
        return CreateInstrumentQueryable(instruments.AsEnumerable());
    }
}
