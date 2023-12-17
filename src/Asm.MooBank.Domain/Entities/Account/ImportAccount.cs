using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class ImportAccount
{
    public Guid AccountId { get; set; }

    public required int ImporterTypeId { get; set; }

    public ImporterType ImporterType { get; set; } = null!;
}
