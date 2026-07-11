namespace Asm.MooBank.Abs;

public interface IAbsClient
{
    Task<IEnumerable<CpiChange>> GetCpiChanges(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default);
}
