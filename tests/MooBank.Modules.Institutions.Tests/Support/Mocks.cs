using System.Linq.Expressions;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.Security;

namespace Asm.MooBank.Modules.Institutions.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        InstitutionRepositoryMock = new Mock<IInstitutionRepository>();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<IInstitutionRepository> InstitutionRepositoryMock { get; }
}
