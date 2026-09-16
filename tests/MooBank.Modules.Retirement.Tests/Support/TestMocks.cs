#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Retirement;
using Asm.MooBank.Modules.Retirement.Services;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Retirement.Tests.Support;

internal class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        RetirementRepositoryMock = new Mock<IRetirementRepository>();

        // The create handler reloads the plan after saving, because members are added by user id
        // and nothing loads the User. Hydrated on each call, so a member added during the handler
        // is covered too.
        RetirementRepositoryMock
            .Setup(r => r.Get(It.IsAny<Guid>(), It.IsAny<ISpecification<RetirementPlan>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => TestEntities.WithUsers(AddedPlan!));
        ProjectionEngineMock = new Mock<IRetirementProjectionEngine>();

        // The guard is exercised in its own tests; handler tests let every member through.
        MemberGuardMock = new Mock<IMemberGuard>();
        MemberGuardMock.Setup(g => g.Assert(It.IsAny<IEnumerable<Asm.MooBank.Modules.Retirement.Models.RetirementPlanMember>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        User = TestEntities.CreateUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IRetirementRepository> RetirementRepositoryMock { get; }

    /// <summary>
    /// The plan passed to Add, taken from the invocation log rather than a callback so a test that
    /// sets up Add itself does not displace it.
    /// </summary>
    private RetirementPlan? AddedPlan =>
        RetirementRepositoryMock.Invocations
            .Where(invocation => invocation.Method.Name == nameof(IRetirementRepository.Add))
            .Select(invocation => (RetirementPlan)invocation.Arguments[0])
            .LastOrDefault();

    public Mock<IRetirementProjectionEngine> ProjectionEngineMock { get; }

    public Mock<IMemberGuard> MemberGuardMock { get; }

    public User User { get; private set; }

    public void SetUser(User user) => User = user;
}
