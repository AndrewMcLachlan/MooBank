#nullable enable
using Asm.MooBank.Domain.Entities.Reports;

namespace Asm.MooBank.Modules.Reports.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        ReportRepositoryMock = new Mock<IReportRepository>();
    }

    public Mock<IReportRepository> ReportRepositoryMock { get; }
}
