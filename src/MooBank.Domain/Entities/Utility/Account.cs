using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Account", Schema = "utilities")]
[AggregateRoot]
public class Account(Guid id) : Instrument.Instrument(id)
{
    public Account() : this(Guid.Empty)
    {
    }

    public static Account Create(string name, string? description, string currency, bool shareWithFamily, UtilityType utilityType, string accountNumber, int? institutionId)
    {
        var account = new Account
        {
            Name = name,
            Description = description,
            Currency = currency,
            Controller = Controller.Manual,
            ShareWithFamily = shareWithFamily,
            UtilityType = utilityType,
            AccountNumber = accountNumber,
            InstitutionId = institutionId,
        };

        account.MarkCreated();

        return account;
    }

    [MaxLength(15)]
    public required string AccountNumber { get; set; }

    public int? InstitutionId { get; set; }

    [Column("UtilityTypeId")]
    public UtilityType UtilityType { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = [];
}
