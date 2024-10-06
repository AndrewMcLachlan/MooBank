using System.ComponentModel;

namespace Asm.MooBank.Models;

[DisplayName("ListItem")]
public record ListItem<TKey>
{
    public required TKey Id { get; set; }

    public required string Name { get; set; }
}
