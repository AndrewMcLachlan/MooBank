using System.Collections;
using System.Linq.Expressions;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;

namespace Asm.MooBank.Core.Tests.Support;

public class Mocks
{
    public Mocks()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        UnitOfWorkMock.Setup(uow => uow.SaveChangesAsync(default)).ReturnsAsync(1);

        SecurityMock = new Mock<ISecurity>();
        SecurityMock.Setup(s => s.AssertGroupPermission(It.IsAny<Guid>()));
        SecurityMock.Setup(s => s.AssertFamilyPermission(It.IsAny<Guid>()));

        CurrencyConverterMock = new Mock<ICurrencyConverter>();
        CurrencyConverterMock.Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns<decimal, string>((amount, currency) => amount);

        HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        HttpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

        ReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
        ReferenceDataRepositoryMock.Setup(r => r.GetExchangeRates(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ExchangeRate { From = "AUD", To = "USD", Rate = 0.65m },
                new ExchangeRate { From = "AUD", To = "EUR", Rate = 0.60m },
                new ExchangeRate { From = "GBP", To = "AUD", Rate = 1.90m },
            ]);

        HybridCacheMock = new Mock<HybridCache>();
    }

    public Mock<IUnitOfWork> UnitOfWorkMock { get; }

    public Mock<ISecurity> SecurityMock { get; }

    public Mock<ICurrencyConverter> CurrencyConverterMock { get; }

    public Mock<IHttpContextAccessor> HttpContextAccessorMock { get; }

    public Mock<IReferenceDataRepository> ReferenceDataRepositoryMock { get; }

    public Mock<HybridCache> HybridCacheMock { get; }

    // Helper to configure security to fail for specific operations
    public void SecurityFail(Expression<Action<ISecurity>> expression)
    {
        SecurityMock.Setup(expression).Throws(new NotAuthorisedException());
    }
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
