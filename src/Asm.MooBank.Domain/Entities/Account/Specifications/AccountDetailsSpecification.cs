﻿using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;

public class AccountDetailsSpecification : ISpecification<InstitutionAccount>
{
    public IQueryable<InstitutionAccount> Apply(IQueryable<InstitutionAccount> query) =>
        query.Include(a => a.AccountHolders).Include(a => a.AccountViewers);
}
