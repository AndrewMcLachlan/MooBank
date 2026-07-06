namespace Asm.MooBank.Domain.Entities.Transactions;

/// <summary>
/// A lightweight projection of a transaction used when only the description is needed
/// (e.g. rule matching), avoiding loading and tracking full entities.
/// </summary>
public sealed record TransactionDescription(Guid Id, string? Description);
