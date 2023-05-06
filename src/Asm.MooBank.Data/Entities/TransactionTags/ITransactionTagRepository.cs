﻿using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.TransactionTags;

public interface ITransactionTagRepository : IDeletableRepository<TransactionTag, int>
{
    void AddSettings(TransactionTag transactionTag);

    Task<IEnumerable<TransactionTag>> Get(IEnumerable<int> tagIds);

    Task<TransactionTag> Get(int id, bool includeSubTags = false);
}
