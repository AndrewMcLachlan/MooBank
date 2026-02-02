#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Bogus;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;
using DomainRule = Asm.MooBank.Domain.Entities.Instrument.Rule;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

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
        IEnumerable<DomainRule>? rules = null)
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

        return instrument;
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
