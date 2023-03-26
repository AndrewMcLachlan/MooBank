namespace Asm.MooBank.Models;

public partial record Account
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required DateTimeOffset BalanceDate { get; set; }

    public required decimal CurrentBalance { get; set; }

    public string? AccountType { get; set; }

    public Guid? AccountGroupId { get; set; }
}
