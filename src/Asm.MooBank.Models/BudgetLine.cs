namespace Asm.MooBank.Models;

public partial record BudgetLine
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int TagId { get; set; }

    public decimal Amount { get; set; }

    public short Month { get; set; }

    public bool Income { get; set; }
}
