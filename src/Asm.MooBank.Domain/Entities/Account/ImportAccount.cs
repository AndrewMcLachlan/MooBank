using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class ImportAccount
{
    [Key]
    public Guid AccountId { get; set; }

    public required int ImporterTypeId { get; set; }

    [ForeignKey(nameof(ImporterTypeId))]
    public ImporterType ImporterType { get; set; } = null!;
}
