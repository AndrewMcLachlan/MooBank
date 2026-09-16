using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.User;

[PrimaryKey(nameof(UserId), nameof(Last4Digits))]
public partial class UserCard
{
    public required Guid UserId { get; set; }

    public required short Last4Digits { get; set; }

    public string? Name { get; set; }

    [ForeignKey(nameof(UserId))]
    [Navigation]
    public partial User User { get; set; }
}
