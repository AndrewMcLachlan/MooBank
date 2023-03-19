namespace Asm.MooBank.Models;

public partial record TransactionTag
{
    public int Id { get; set; }

    public string Name { get; set; }

    public IEnumerable<TransactionTag> Tags { get; set; }
}
