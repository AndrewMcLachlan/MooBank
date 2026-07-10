#nullable enable
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Reports.Tests.Support;

public class TestMocks
{
    public TestMocks()
    {
        ReportRepositoryMock = new Mock<IReportRepository>();

        User = new User
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Currency = "AUD",
            FamilyId = Guid.NewGuid(),
            Accounts = [],
            SharedAccounts = [],
            Groups = [],
        };
    }

    public Mock<IReportRepository> ReportRepositoryMock { get; }

    public User User { get; }
}
