namespace Asm.MooBank.Models;

public partial record InstitutionAccount : Account
{
    public bool IncludeInPosition { get; set; }

    public new AccountType AccountType { get; set; }

    public AccountController Controller { get; set; }

    public int? ImporterTypeId { get; set; }

    public bool IsPrimary { get; set; }

    public bool ShareWithFamily { get; set; }

    public int InstitutionId { get; set; }

    public IEnumerable<VirtualAccount> VirtualAccounts { get; set; } = Enumerable.Empty<VirtualAccount>();

    public decimal VirtualAccountRemainingBalance
    {
        get => CurrentBalance - (VirtualAccounts?.Sum(v => v.CurrentBalance) ?? 0);
    }
}
