#nullable enable
using Asm.MooBank.Domain.Entities.Reports;

namespace Asm.MooBank.Modules.Reports.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        ReportReaderMock = new Mock<IReportReader>();
    }

    public Mock<IReportReader> ReportReaderMock { get; }
}
