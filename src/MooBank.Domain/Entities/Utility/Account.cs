
namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Account", Schema = "utilities")]
[AggregateRoot]
public class Account : Instrument.Instrument
{
    internal Account(Guid id) : base(id)
    {
    }

    // For EF materialisation only. Construct through Create.
    internal Account() : this(Guid.Empty)
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

    /// <summary>
    /// Replaces the account's details with those supplied.
    /// </summary>
    /// <remarks>
    /// <see cref="UtilityType"/> is fixed for the life of the account: charge types are scoped to a
    /// utility type, so the bills already held only read correctly against the one they were
    /// entered under.
    /// </remarks>
    public void Update(string name, string? description, string accountNumber, bool shareWithFamily)
    {
        Name = name;
        Description = description;
        AccountNumber = accountNumber;
        ShareWithFamily = shareWithFamily;

        MarkUpdated();
    }
}
