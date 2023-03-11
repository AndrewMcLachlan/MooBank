﻿using Asm.MooBank.Data.Entities;
using Asm.MooBank.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services
{
    public class TransactionTagRuleRepository : DataRepository, ITransactionTagRuleRepository
    {
        private readonly ISecurityRepository _security;
        private readonly ITransactionTagRepository _transactionTags;

        public TransactionTagRuleRepository(BankPlusContext bankPlusContext, ISecurityRepository security, ITransactionTagRepository transactionTags) : base(bankPlusContext)
        {
            _security = security;
            _transactionTags = transactionTags;
        }

        public async Task<Models.TransactionTagRule> Create(Guid accountId, string contains, IEnumerable<int> tagIds)
        {
            _security.AssertPermission(accountId);

            var rule = new TransactionTagRule
            {
                AccountId = accountId,
                Contains = contains,
                TransactionTags = tagIds.Select(t => new TransactionTag { TransactionTagId = t }).ToList(),
            };

            DataContext.Add(rule);

            await DataContext.SaveChangesAsync();

            return (Models.TransactionTagRule)rule;
        }

        public async Task<Models.TransactionTagRule> Create(Guid accountId, string contains, IEnumerable<Models.TransactionTag> tags)
        {
            _security.AssertPermission(accountId);

            var rule = new TransactionTagRule
            {
                AccountId = accountId,
                Contains = contains,
                TransactionTags =  (await _transactionTags.Get(tags.Select(t => t.Id))).ToList(),
            };

            DataContext.Add(rule);

            await DataContext.SaveChangesAsync();

            return await Get(accountId, rule.TransactionTagRuleId);
        }

        public async Task Delete(Guid accountId, int id)
        {
            var rule = await GetEntity(accountId, id);

            DataContext.TransactionTagRules.Remove(rule);

            await DataContext.SaveChangesAsync();
        }

        public async Task<Models.TransactionTagRule> Get(Guid accountId, int id)
        {
            return (Models.TransactionTagRule) await GetEntity(accountId, id);
        }
        public async Task<IEnumerable<Models.TransactionTagRule>> Get(Guid accountId)
        {
            _security.AssertPermission(accountId);

            return await DataContext.TransactionTagRules.Include(t => t.TransactionTags).Where(t => t.AccountId == accountId).OrderBy(t => t.Contains).Select(t => (Models.TransactionTagRule)t).ToListAsync();
        }

        public async Task<Models.TransactionTagRule> AddTransactionTag(Guid accountId, int id, int tagId)
        {
            var entity = await GetEntity(accountId, id);

            if (entity == null) throw new NotFoundException($"Transaction tag rule with ID {id} not found");

            if (entity.TransactionTags.Any(t => t.TransactionTagId == tagId)) throw new ExistsException("Cannot add tag, it already exists");

            entity.TransactionTags.Add(DataContext.TransactionTags.Single(t => t.TransactionTagId == tagId));

            await DataContext.SaveChangesAsync();

            return (Models.TransactionTagRule)entity;
        }

        public async Task<Models.TransactionTagRule> RemoveTransactionTag(Guid accountId, int id, int tagId)
        {
            var entity = await GetEntity(accountId, id);

            var tag = entity.TransactionTags.SingleOrDefault(t => t.TransactionTagId == tagId);

            if (tag == null) throw new NotFoundException($"Tag with id {id} was not found");

            entity.TransactionTags.Remove(tag);

            await DataContext.SaveChangesAsync();

            return (Models.TransactionTagRule)entity;
        }

        private async Task<TransactionTagRule> GetEntity(Guid accountId, int id)
        {
            TransactionTagRule entity = await DataContext.TransactionTagRules.Include(t => t.TransactionTags).SingleOrDefaultAsync(t => t.TransactionTagRuleId == id && t.AccountId == accountId);

            if (entity == null) throw new NotFoundException($"Transaction tag rule with ID {id} was not found");

            _security.AssertPermission(entity.AccountId);

            return entity;
        }
    }
}