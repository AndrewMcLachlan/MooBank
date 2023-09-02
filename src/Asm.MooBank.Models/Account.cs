namespace Asm.MooBank.Models;

public partial record Account
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset BalanceDate { get; set; } = DateTimeOffset.Now;

    public DateOnly? LastTransaction { get; set; }

    public required decimal CurrentBalance { get; set; }

    public decimal CalculatedBalance { get; set; }

    public string? AccountType { get; set; }

    public Guid? AccountGroupId { get; set; }
}
