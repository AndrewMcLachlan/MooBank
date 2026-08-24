namespace Asm.MooBank.Services.DemoData;

/// <summary>
/// The instruments the demo data job is allowed to write to.
/// </summary>
/// <remarks>
/// The job runs in production, beside real accounts, so it writes only where it is pointed. Absence
/// is the off switch: an id left unset means that piece is skipped, which leaves development,
/// staging and any environment restored from a backup inert without a separate enabled flag to fall
/// out of step with the ids.
/// </remarks>
public class DemoDataOptions
{
    public const string SectionName = "DemoData";

    public Guid? CheckingAccountId { get; set; }

    public Guid? SavingsAccountId { get; set; }

    public Guid? MortgageAccountId { get; set; }

    public Guid? SuperAccountId { get; set; }

    public Guid? LoanAccountId { get; set; }

    public Guid? ElectricityAccountId { get; set; }

    public Guid? WaterAccountId { get; set; }

    /// <summary>
    /// Whether any instrument at all has been configured.
    /// </summary>
    public bool IsConfigured =>
        CheckingAccountId is not null || SavingsAccountId is not null || MortgageAccountId is not null ||
        SuperAccountId is not null || LoanAccountId is not null || ElectricityAccountId is not null ||
        WaterAccountId is not null;
}
