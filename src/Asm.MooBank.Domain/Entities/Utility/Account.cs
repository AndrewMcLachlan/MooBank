namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Account", Schema = "utilities")]
[AggregateRoot]
public class Account(Guid id) : Instrument.Instrument(id)
{
    public Account() : this(Guid.Empty)
    {
    }

    [MaxLength(15)]
    public required string AccountNumber { get; set; }

    public int? InstitutionId { get; set; }

    [Column("UtilityTypeId")]
    public UtilityType UtilityType { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = [];
}
