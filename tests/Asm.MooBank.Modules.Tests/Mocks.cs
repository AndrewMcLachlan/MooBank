using System.Collections;
using System.Linq.Expressions;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Tests.Extensions;
using Asm.MooBank.Security;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Tests;
internal class Mocks
{
    public Mocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(UnitOfWorkMock => UnitOfWorkMock.SaveChangesAsync(default)).ReturnsAsync(1);

        SecurityMock = new Mock<ISecurity>();
        SecurityMock.Setup(s => s.AssertInstrumentPermission(It.IsAny<Instrument>()));
        SecurityMock.Setup(s => s.AssertInstrumentPermission(It.IsAny<Guid>()));
        SecurityMock.Setup(s => s.AssertGroupPermission(It.IsAny<Guid>()));
        SecurityMock.Setup(s => s.AssertFamilyPermission(It.IsAny<Guid>()));

        SecurityMock.Fail(s => s.AssertInstrumentPermission(Models.InvalidAccountId));
        SecurityMock.Fail(s => s.AssertGroupPermission(Models.InvalidGroupId));
        SecurityMock.Fail(s => s.AssertFamilyPermission(Models.InvalidAccountFamilyId));

        CurrencyConverterMock = new Mock<ICurrencyConverter>();
        CurrencyConverterMock.Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>())).Returns<decimal, string>((amount, currency) => amount);
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }

    public Mock<ISecurity> SecurityMock { get; private set; }

    public Mock<ICurrencyConverter> CurrencyConverterMock { get; private set; }
}

public class MockAsyncEnumerable<T>(IEnumerable<T> values) : IQueryable<T>, IAsyncEnumerable<T>
{
    public Type ElementType => values.AsQueryable().ElementType;

    public Expression Expression => values.AsQueryable().Expression;

    public IQueryProvider Provider => values.AsQueryable().Provider;

    public IAsyncEnumerable<T> AsAsyncEnumerable() => this;

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        values.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken);

    public IEnumerator<T> GetEnumerator() => values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
}
