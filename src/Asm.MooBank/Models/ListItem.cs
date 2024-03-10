namespace Asm.MooBank.Models;
public record ListItem<TKey>
{
    public required TKey Id { get; set; }

    public required string Name { get; set; }
}
