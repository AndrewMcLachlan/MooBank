using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

/// <summary>
/// The least an account-based pension must pay out in a year, as a share of the balance, for
/// someone of a given age.
/// </summary>
/// <remarks>
/// Legislated rather than assumed (ATO Schedule 7), which is why a projection has to honour it: a
/// balance left to compound past this point is one the law would not let you keep. Held as data
/// because the rates have been changed before — they were halved for four years from 2019-20.
/// </remarks>
[PrimaryKey(nameof(MinAge))]
public class MinimumDrawdownRate
{
    /// <summary>
    /// The age this band starts at. It applies until the next band begins.
    /// </summary>
    public byte MinAge { get; set; }

    [Precision(6, 4)]
    public decimal Rate { get; set; }
}
