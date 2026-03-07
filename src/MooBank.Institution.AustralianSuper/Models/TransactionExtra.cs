using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Institution.AustralianSuper.Models;

public partial class TransactionExtra
{
    public decimal? SGContributions { get; set; }

    public decimal? EmployerAdditional { get; set; }

    public decimal? SalarySacrifice { get; set; }

    public decimal? MemberAdditional { get; set; }
}
