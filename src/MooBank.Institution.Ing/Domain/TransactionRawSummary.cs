namespace Asm.MooBank.Institution.Ing.Domain;

/// <summary>
/// A lightweight projection of <see cref="TransactionRaw"/> used for duplicate detection during import.
/// </summary>
internal sealed record TransactionRawSummary(string? Description, DateOnly Date, decimal? Credit, decimal? Debit);
