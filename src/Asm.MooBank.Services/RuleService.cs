using Asm.Domain;
using Asm.MooBank.Models;
using Microsoft.Identity.Client;
using ITransactionTagRepository = Asm.MooBank.Domain.Entities.Tag.ITransactionTagRepository;
using IRuleRepository = Asm.MooBank.Domain.Entities.Account.IRuleRepository;

namespace Asm.MooBank.Services;

public interface IRuleService
{
    //Task<Rule> Create(Guid accountId, string contains, string? description, IEnumerable<int> tagIds, CancellationToken cancellationToken = default);

    Task<Rule> Create(Guid accountId, string contains, string? description, IEnumerable<Tag> tagIds, CancellationToken cancellationToken = default);

    Task<Rule> Get(Guid accountId, int id);

    Task<IEnumerable<Rule>> Get(Guid accountId, CancellationToken cancellationToken = default);

    Task Delete(Guid accountId, int id, CancellationToken cancellationToken = default);

    Task<Rule> AddTransactionTag(Guid accountId, int id, int tagId);

    Task<Rule> RemoveTransactionTag(Guid accountId, int id, int tagId);
}

public class RuleService : ServiceBase, IRuleService
{
    private readonly ISecurity _security;
    private readonly ITransactionTagRepository _transactionTags;
    private readonly IRuleRepository _transactionTagRuleRepository;

    public RuleService(IUnitOfWork unitOfWork, ISecurity security, IRuleRepository transactionTagRuleRepository, ITransactionTagRepository transactionTags) : base(unitOfWork)
    {
        _security = security;
        _transactionTagRuleRepository = transactionTagRuleRepository;
        _transactionTags = transactionTags;
    }

   /* public async Task<Rule> Create(Guid accountId, string contains, string? description, IEnumerable<int> tagIds, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        var rule = new Domain.Entities.Account.Rule
        {
            AccountId = accountId,
            Contains = contains,
            Description = description,
            Tags = tagIds.Select(t => new Domain.Entities.Tag.Tag { Id = t }).ToList(),
        };

        _transactionTagRuleRepository.Add(rule);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return rule;
    }*/

    public async Task<Rule> Create(Guid accountId, string contains, string? description, IEnumerable<Tag> tags, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        var rule = new Domain.Entities.Account.Rule
        {
            AccountId = accountId,
            Contains = contains,
            Description = description,
            Tags = (await _transactionTags.Get(tags.Select(t => t.Id))).ToList(),
        };

        _transactionTagRuleRepository.Add(rule);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Get(accountId, rule.Id);
    }*/

    public async Task Delete(Guid accountId, int id, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        await _transactionTagRuleRepository.Delete(accountId, id, cancellationToken);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Rule> Get(Guid accountId, int id) => await GetEntity(accountId, id);

    public async Task<IEnumerable<Rule>> Get(Guid accountId, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionTagRuleRepository.GetForAccount(accountId).ToModelAsync(cancellationToken);
    }

    public async Task<Rule> AddTransactionTag(Guid accountId, int id, int tagId)
    {
        var entity = await GetEntity(accountId, id);

        if (entity == null) throw new NotFoundException($"Transaction tag rule with ID {id} not found");

        if (entity.Tags.Any(t => t.Id == tagId)) throw new ExistsException("Cannot add tag, it already exists");

        entity.Tags.Add(await _transactionTags.Get(tagId));

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    public async Task<Rule> RemoveTransactionTag(Guid accountId, int id, int tagId)
    {
        var entity = await GetEntity(accountId, id);

        var tag = entity.Tags.SingleOrDefault(t => t.Id == tagId);

        if (tag == null) throw new NotFoundException($"Tag with id {id} was not found");

        entity.Tags.Remove(tag);

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    private async Task<Domain.Entities.Account.Rule> GetEntity(Guid accountId, int id)
    {
        _security.AssertAccountPermission(accountId);

        Domain.Entities.Account.Rule entity = await _transactionTagRuleRepository.Get(accountId, id);

        return entity;
    }
}
