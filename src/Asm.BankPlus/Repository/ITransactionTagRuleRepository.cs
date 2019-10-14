﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Repository
{
    public interface ITransactionTagRuleRepository
    {
        Task<TransactionTagRule> Create(Guid accountId, string contains, IEnumerable<int> tagIds);

        Task<TransactionTagRule> Get(Guid accountId, int id);

        Task<IEnumerable<TransactionTagRule>> Get(Guid accountId);

        Task Delete(int id);

        Task<TransactionTagRule> AddTransactionTag(Guid accountId, int id, int tagId);

        Task<TransactionTagRule> RemoveTransactionTag(Guid accountId, int id, int tagId);


    }
}
