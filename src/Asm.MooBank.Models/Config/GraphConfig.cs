namespace Asm.MooBank.Models.Config;

public record GraphConfig
{
    public string TenantId { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }
}
