﻿using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

public class IncludeSplitsAndOffsetsSpecification : ISpecification<Transaction>
{
    public IQueryable<Transaction> Apply(IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(t => t.OffsetBy).Include(t => t.OffsetFor);
}
