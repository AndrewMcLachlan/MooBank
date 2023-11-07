﻿using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Budget;

public interface IBudgetRepository : IDeleteRepository<Budget, Guid>
{
    Task<Budget> GetByYear(Guid accountId, short year, CancellationToken cancellationToken = default);
    Task<Budget> GetOrCreate(Guid accountId, short year, CancellationToken cancellationToken = default);

    BudgetLine AddLine(BudgetLine entity);

    void DeleteLine(Guid id);
}