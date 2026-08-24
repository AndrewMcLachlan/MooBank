using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserEntity = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Services.DemoData;

public interface IDemoDataService
{
    /// <summary>
    /// Extends the configured demo instruments by one month: the month that has just ended.
    /// </summary>
    Task Extend(CancellationToken cancellationToken = default);
}

/// <summary>
/// Decides whether the demo data job should run, and as whom.
/// </summary>
/// <remarks>
/// Kept apart from <see cref="DemoDataWriter"/> because of ordering. The account repositories take
/// the current user in their constructors, and the tag query filter reads the current family, so
/// the identity has to be in place before any of that is resolved -- not merely before it is used.
/// This class therefore resolves only what it can read without a user, works out who the demo
/// family is, and opens a fresh scope for the writer.
/// </remarks>
internal class DemoDataService(
    IOptions<DemoDataOptions> options,
    IQueryable<InstrumentOwner> instrumentOwners,
    IQueryable<UserEntity> users,
    IServiceScopeFactory scopeFactory,
    ILogger<DemoDataService> logger) : IDemoDataService
{
    public async Task Extend(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        if (!settings.IsConfigured)
        {
            logger.LogInformation("No demo instruments are configured. Nothing to do.");
            return;
        }

        if (settings.CheckingAccountId is null)
        {
            logger.LogWarning("Demo data is configured but no checking account is set. Every other instrument is derived from it, so there is nothing to extend.");
            return;
        }

        var owner = await DemoUser(settings.CheckingAccountId.Value, cancellationToken);

        if (owner is null) return;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var month = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

        logger.LogInformation("Extending demo data for {Month:yyyy-MM}.", month);

        using var scope = scopeFactory.CreateScope();

        scope.ServiceProvider.GetRequiredService<ISettableUserDataProvider>().SetUser(owner);

        var writer = scope.ServiceProvider.GetRequiredService<IDemoDataWriter>();

        await writer.Extend(month, cancellationToken);
    }

    /// <summary>
    /// The owner of the configured checking account, whose identity the run adopts.
    /// </summary>
    /// <remarks>
    /// An id that resolves to nothing is reported rather than passed over: a mistyped instrument
    /// must not read as "nothing to do".
    /// </remarks>
    private async Task<Models.User?> DemoUser(Guid checkingId, CancellationToken cancellationToken)
    {
        var ownerId = await instrumentOwners
            .Where(o => o.InstrumentId == checkingId)
            .Select(o => o.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ownerId == Guid.Empty)
        {
            logger.LogError("The configured demo checking account {InstrumentId} does not exist, or has no owner.", checkingId);
            return null;
        }

        var owner = await users
            .Where(u => u.Id == ownerId)
            .Select(u => new Models.User
            {
                Id = u.Id,
                EmailAddress = u.EmailAddress,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Currency = u.Currency,
                FamilyId = u.FamilyId,
                PrimaryAccountId = u.PrimaryAccountId,
                // The utility account repository scopes itself to these, so the bills step needs
                // the demo family's instruments listed here to see its own accounts.
                Accounts = u.InstrumentOwners.Select(io => io.InstrumentId),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (owner is null)
        {
            logger.LogError("The owner of the demo checking account could not be loaded.");
        }

        return owner;
    }
}
