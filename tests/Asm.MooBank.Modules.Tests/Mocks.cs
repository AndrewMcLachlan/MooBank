using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Modules.Tests.Extensions;
using Asm.MooBank.Security;
using Asm.MooBank.Services;
using Moq;

namespace Asm.MooBank.Modules.Tests;
internal class Mocks
{
    public Mocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(UnitOfWorkMock => UnitOfWorkMock.SaveChangesAsync(default)).ReturnsAsync(1);

        SecurityMock = new Mock<ISecurity>();
        SecurityMock.Setup(s => s.AssertAccountPermission(It.IsAny<Domain.Entities.Account.Instrument>()));
        SecurityMock.Setup(s => s.AssertAccountPermission(It.IsAny<Guid>()));
        SecurityMock.Setup(s => s.AssertAccountGroupPermission(It.IsAny<Guid>()));
        SecurityMock.Setup(s => s.AssertFamilyPermission(It.IsAny<Guid>()));

        SecurityMock.Fail(s => s.AssertAccountPermission(Models.InvalidAccountId));
        SecurityMock.Fail(s => s.AssertAccountGroupPermission(Models.InvalidAccountGroupId));
        SecurityMock.Fail(s => s.AssertFamilyPermission(Models.InvalidAccountFamilyId));

        CurrencyConverterMock = new Mock<ICurrencyConverter>();
        CurrencyConverterMock.Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>())).Returns<decimal, string>((amount, currency) => amount);
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }

    public Mock<ISecurity> SecurityMock { get; private set; }

    public Mock<ICurrencyConverter> CurrencyConverterMock { get; private set; }
}

