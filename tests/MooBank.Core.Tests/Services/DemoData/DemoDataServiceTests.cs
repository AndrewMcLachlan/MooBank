using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Security;
using Asm.MooBank.Services.DemoData;
using Asm.Testing.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UserEntity = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Core.Tests.Services.DemoData;

/// <summary>
/// Unit tests for <see cref="DemoDataService"/>, which decides whether the demo data job runs and
/// as whom. The service runs in production beside real accounts, so the cases that matter are the
/// ones where it must decline.
/// </summary>
public class DemoDataServiceTests
{
    private static readonly Guid CheckingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid FamilyId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly Mock<IDemoDataWriter> _writer = new();
    private readonly Mock<ISettableUserDataProvider> _userDataProvider = new();

    /// <summary>
    /// Given no demo instruments are configured
    /// When the job runs
    /// Then nothing is written and no identity is adopted.
    /// </summary>
    /// <remarks>
    /// The whole safety story rests on this: an environment that has not named its demo instruments
    /// is inert, so a restored copy of production cannot start inventing transactions.
    /// </remarks>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_NotConfigured_WritesNothing()
    {
        var service = CreateService(new DemoDataOptions());

        await service.Extend(TestContext.Current.CancellationToken);

        AssertNothingWritten();
    }

    /// <summary>
    /// Given instruments are configured but the checking account is not among them
    /// When the job runs
    /// Then nothing is written, because every other instrument is derived from checking.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_NoCheckingAccountConfigured_WritesNothing()
    {
        var service = CreateService(new DemoDataOptions { MortgageAccountId = Guid.NewGuid() });

        await service.Extend(TestContext.Current.CancellationToken);

        AssertNothingWritten();
    }

    /// <summary>
    /// Given a configured checking account id that matches no instrument
    /// When the job runs
    /// Then nothing is written.
    /// </summary>
    /// <remarks>
    /// A mistyped id must not read as "nothing to do" -- it stops the run rather than letting the
    /// derived pieces go looking for repayments on an account that is not there.
    /// </remarks>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_CheckingAccountDoesNotExist_WritesNothing()
    {
        var service = CreateService(new DemoDataOptions { CheckingAccountId = CheckingId }, owners: []);

        await service.Extend(TestContext.Current.CancellationToken);

        AssertNothingWritten();
    }

    /// <summary>
    /// Given a configured demo checking account
    /// When the job runs
    /// Then the demo family's identity is adopted before the writer is asked to do anything.
    /// </summary>
    /// <remarks>
    /// Ordering, not decoration: the account repositories take the current user in their own
    /// constructors and the tag filter reads the current family, so a writer resolved first would
    /// throw or quietly find no tags.
    /// </remarks>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_Configured_AdoptsTheDemoIdentityThenWrites()
    {
        var service = CreateService(new DemoDataOptions { CheckingAccountId = CheckingId });

        await service.Extend(TestContext.Current.CancellationToken);

        _userDataProvider.Verify(u => u.SetUser(It.Is<Asm.MooBank.Models.User>(user => user.Id == OwnerId && user.FamilyId == FamilyId)), Times.Once);
        _writer.Verify(w => w.Extend(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given today's date
    /// When the job runs
    /// Then the month it fills is the one that has just ended.
    /// </summary>
    /// <remarks>
    /// Computed by subtracting a month from the first of this one rather than by arithmetic on
    /// today's day number, so a run on 31 March fills February rather than skipping it.
    /// </remarks>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_Always_FillsThePreviousMonth()
    {
        var service = CreateService(new DemoDataOptions { CheckingAccountId = CheckingId });
        var today = DateOnly.FromDateTime(DateTime.Today);
        var expected = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

        await service.Extend(TestContext.Current.CancellationToken);

        _writer.Verify(w => w.Extend(expected, It.IsAny<CancellationToken>()), Times.Once);
    }

    private void AssertNothingWritten()
    {
        _writer.Verify(w => w.Extend(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        _userDataProvider.Verify(u => u.SetUser(It.IsAny<Asm.MooBank.Models.User>()), Times.Never);
    }

    private DemoDataService CreateService(DemoDataOptions options, IEnumerable<InstrumentOwner>? owners = null)
    {
        owners ??= [new InstrumentOwner { InstrumentId = CheckingId, UserId = OwnerId }];

        var user = new UserEntity(OwnerId) { EmailAddress = "demo@example.com", FamilyId = FamilyId };

        var services = new ServiceCollection()
            .AddScoped(_ => _userDataProvider.Object)
            .AddScoped(_ => _writer.Object)
            .BuildServiceProvider();

        return new DemoDataService(
            Options.Create(options),
            MockDbSetFactory.CreateQueryable(owners),
            MockDbSetFactory.CreateQueryable<UserEntity>([user]),
            services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<DemoDataService>.Instance);
    }
}
