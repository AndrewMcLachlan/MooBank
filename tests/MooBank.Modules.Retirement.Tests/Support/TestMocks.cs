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
        ProjectionEngineMock = new Mock<IRetirementProjectionEngine>();

        // The guard is exercised in its own tests; handler tests let every member through.
        MemberGuardMock = new Mock<IMemberGuard>();
        MemberGuardMock.Setup(g => g.Assert(It.IsAny<IEnumerable<Asm.MooBank.Modules.Retirement.Models.RetirementPlanMember>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        User = TestEntities.CreateUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IRetirementRepository> RetirementRepositoryMock { get; }

    public Mock<IRetirementProjectionEngine> ProjectionEngineMock { get; }

    public Mock<IMemberGuard> MemberGuardMock { get; }

    public User User { get; private set; }

    public void SetUser(User user) => User = user;
}
