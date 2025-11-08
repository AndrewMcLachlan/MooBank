namespace Asm.MooBank.Modules.Accounts.Models.Account;

public partial record InstitutionAccount
{
    public Guid Id { get; init; }

    public int? ImporterTypeId { get; init; }

    public int InstitutionId { get; init; }

    public required string Name { get; init; }

    public DateOnly OpenedDate { get; set; }

    public DateOnly? ClosedDate { get; set; }
}

public static class InstitutionAccountExtensions
{
    public static InstitutionAccount ToModel(this Domain.Entities.Account.InstitutionAccount account) => new()
    {
        Id = account.Id,
        ImporterTypeId = account.ImporterTypeId,
        Name = account.Name,
        InstitutionId = account.InstitutionId,
        OpenedDate = account.OpenedDate,
        ClosedDate = account.ClosedDate,
    };

    public static Domain.Entities.Account.InstitutionAccount ToEntity(this InstitutionAccount account) => new(account.Id == Guid.Empty ? Guid.NewGuid() : account.Id)
    {
        InstitutionId = account.InstitutionId,
        Name = account.Name,
        ImporterTypeId = account.ImporterTypeId,
        OpenedDate = account.OpenedDate,
        ClosedDate = account.ClosedDate,
    };

    public static IEnumerable<InstitutionAccount> ToModel(this IReadOnlyCollection<Domain.Entities.Account.InstitutionAccount> entities) => entities.Select(ToModel);

    public static IReadOnlyCollection<Domain.Entities.Account.InstitutionAccount> ToEntity(this IEnumerable<InstitutionAccount> models) => [.. models.Select(ToEntity)];
}
