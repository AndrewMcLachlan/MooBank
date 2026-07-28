#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Retirement;
using Asm.MooBank.Modules.Retirement.Services;
using User = Asm.MooBank.Models.User;

namespace Asm.MooBank.Modules.Retirement.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        RetirementRepositoryMock = new Mock<IRetirementRepository>();
        ProjectionEngineMock = new Mock<IRetirementProjectionEngine>();

        User = TestEntities.CreateUser();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IRetirementRepository> RetirementRepositoryMock { get; }

    public Mock<IRetirementProjectionEngine> ProjectionEngineMock { get; }

    public User User { get; private set; }

    public void SetUser(User user) => User = user;
}
