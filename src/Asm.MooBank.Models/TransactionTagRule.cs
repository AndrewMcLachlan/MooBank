namespace Asm.MooBank.Models;

public partial record TransactionTagRule
{
    public int Id { get; set; }

    public string Contains { get; set; }

    public IEnumerable<TransactionTag> Tags { get; set; }
}
