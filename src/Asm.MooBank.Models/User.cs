namespace Asm.MooBank.Models;

public partial record User
{
    public Guid Id { get; set; }

    public required string EmailAddress { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public required string Currency { get; set; }

    public Guid FamilyId { get; set; }

    public Guid? PrimaryAccountId { get; set; }

    public IEnumerable<Guid> Accounts { get; set; } = [];

    public IEnumerable<Guid> SharedAccounts { get; set; } = [];
}


