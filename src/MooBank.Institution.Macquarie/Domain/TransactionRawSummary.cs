namespace Asm.MooBank.Institution.Macquarie.Domain;

/// <summary>
/// A lightweight projection of <see cref="TransactionRaw"/> used for duplicate detection during import.
/// </summary>
internal sealed record TransactionRawSummary(string? Details, DateOnly Date, decimal? Credit, decimal? Debit, decimal? Balance);
