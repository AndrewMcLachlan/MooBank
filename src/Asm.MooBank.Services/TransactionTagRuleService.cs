﻿using Asm.Domain;
using Asm.MooBank.Models;
using ITransactionTagRuleRepository = Asm.MooBank.Domain.Entities.Account.ITransactionTagRuleRepository;
using ITransactionTagRepository = Asm.MooBank.Domain.Entities.TransactionTags.ITransactionTagRepository;

namespace Asm.MooBank.Services;

public interface ITransactionTagRuleService
{
    Task<TransactionTagRule> Create(Guid accountId, string contains, IEnumerable<int> tagIds);

    Task<TransactionTagRule> Create(Guid accountId, string contains, IEnumerable<TransactionTag> tagIds);

    Task<TransactionTagRule> Get(Guid accountId, int id);

    Task<IEnumerable<TransactionTagRule>> Get(Guid accountId, CancellationToken cancellationToken = default);

    Task Delete(Guid accountId, int id);

    Task<TransactionTagRule> AddTransactionTag(Guid accountId, int id, int tagId);

    Task<TransactionTagRule> RemoveTransactionTag(Guid accountId, int id, int tagId);
}

public class TransactionTagRuleService : ServiceBase, ITransactionTagRuleService
{
    private readonly ISecurityRepository _security;
    private readonly ITransactionTagRepository _transactionTags;
    private readonly ITransactionTagRuleRepository _transactionTagRuleRepository;

    public TransactionTagRuleService(IUnitOfWork unitOfWork, ISecurityRepository security, ITransactionTagRuleRepository transactionTagRuleRepository, ITransactionTagRepository transactionTags) : base(unitOfWork)
    {
        _security = security;
        _transactionTagRuleRepository = transactionTagRuleRepository;
        _transactionTags = transactionTags;
    }

    public async Task<TransactionTagRule> Create(Guid accountId, string contains, IEnumerable<int> tagIds)
    {
        _security.AssertAccountPermission(accountId);

        var rule = new Domain.Entities.Account.TransactionTagRule
        {
            AccountId = accountId,
            Contains = contains,
            TransactionTags = tagIds.Select(t => new Domain.Entities.TransactionTags.TransactionTag { TransactionTagId = t }).ToList(),
        };

        _transactionTagRuleRepository.Add(rule);

        await UnitOfWork.SaveChangesAsync();

        return rule;
    }

    public async Task<Models.TransactionTagRule> Create(Guid accountId, string contains, IEnumerable<TransactionTag> tags)
    {
        _security.AssertAccountPermission(accountId);

        var rule = new Domain.Entities.Account.TransactionTagRule
        {
            AccountId = accountId,
            Contains = contains,
            TransactionTags = (await _transactionTags.Get(tags.Select(t => t.Id))).ToList(),
        };

        _transactionTagRuleRepository.Add(rule);

        await UnitOfWork.SaveChangesAsync();

        return await Get(accountId, rule.TransactionTagRuleId);
    }

    public async Task Delete(Guid accountId, int id)
    {
        var rule = await GetEntity(accountId, id);

        _transactionTagRuleRepository.Delete(rule);

        await UnitOfWork.SaveChangesAsync();
    }

    public async Task<TransactionTagRule> Get(Guid accountId, int id) => await GetEntity(accountId, id);

    public async Task<IEnumerable<TransactionTagRule>> Get(Guid accountId, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionTagRuleRepository.GetForAccount(accountId).ToModelAsync(cancellationToken);
    }

    public async Task<TransactionTagRule> AddTransactionTag(Guid accountId, int id, int tagId)
    {
        var entity = await GetEntity(accountId, id);

        if (entity == null) throw new NotFoundException($"Transaction tag rule with ID {id} not found");

        if (entity.TransactionTags.Any(t => t.TransactionTagId == tagId)) throw new ExistsException("Cannot add tag, it already exists");

        entity.TransactionTags.Add(await _transactionTags.Get(tagId));

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    public async Task<TransactionTagRule> RemoveTransactionTag(Guid accountId, int id, int tagId)
    {
        var entity = await GetEntity(accountId, id);

        var tag = entity.TransactionTags.SingleOrDefault(t => t.TransactionTagId == tagId);

        if (tag == null) throw new NotFoundException($"Tag with id {id} was not found");

        entity.TransactionTags.Remove(tag);

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    private async Task<Domain.Entities.Account.TransactionTagRule> GetEntity(Guid accountId, int id)
    {
        _security.AssertAccountPermission(accountId);

        Domain.Entities.Account.TransactionTagRule entity = await _transactionTagRuleRepository.Get(accountId, id);

        return entity;
    }
}
