namespace Asm.MooBank.Models;

public partial record Account
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset BalanceDate { get; set; }

    public decimal CurrentBalance { get; set; }
}
