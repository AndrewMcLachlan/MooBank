namespace Asm.MooBank.AppHost.Configuration;

/// <summary>
/// Configuration for containerised SQL Server.
/// </summary>
internal record SqlServer
{
    /// <summary>
    /// Gets or sets a value indicating whether the SQL Server container is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets or sets an optional value telling the SQL Server container.
    /// </summary>
    public string? DataBindMount { get; init; }
}
