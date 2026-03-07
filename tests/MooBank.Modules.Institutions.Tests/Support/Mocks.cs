using System.Linq.Expressions;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Security;
using Asm.Security;

namespace Asm.MooBank.Modules.Institutions.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        SecurityMock = new Mock<ISecurity>();
        SecurityMock.Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        InstitutionRepositoryMock = new Mock<IInstitutionRepository>();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

    public Mock<IInstitutionRepository> InstitutionRepositoryMock { get; }

    public void SecurityFailAdmin()
    {
        SecurityMock.Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotAuthorisedException());
    }
}
