namespace Asm.MooBank.Modules.Accounts.Models.InstitutionAccount;

public record UpdateInstitutionAccount
{
    public int InstitutionId { get; init; }

    public int? ImporterTypeId { get; init; }

    public required string Name { get; init; }
}
