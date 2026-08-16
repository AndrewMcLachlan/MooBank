#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.McpTools;
using Asm.MooBank.Modules.Bills.Models;
using Asm.MooBank.Modules.Bills.Queries.Bills;
using Postie.Cqrs.Commands;
using Postie.Cqrs.Queries;
using Bill = Asm.MooBank.Modules.Bills.Models.Bill;

namespace Asm.MooBank.Modules.Bills.Tests.McpTools;

/// <summary>
/// Unit tests for <see cref="BillTools"/>.
/// </summary>
/// <remarks>
/// The tools are thin, so what is worth pinning down is the translation from what a caller sends
/// to the query that is dispatched: the paging a caller does not have to think about, and the
/// filters it does.
/// </remarks>
[Trait("Category", "Unit")]
public class BillToolsTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcher = new();

    private GetAll? _dispatched;

    public BillToolsTests()
    {
        _queryDispatcher
            .Setup(d => d.Dispatch(It.IsAny<GetAll>(), It.IsAny<CancellationToken>()))
            .Callback<IQuery<PagedResult<Bill>>, CancellationToken>((query, _) => _dispatched = (GetAll)query)
            .Returns(ValueTask.FromResult(new PagedResult<Bill> { Results = [], Total = 0 }));
    }

    private BillTools CreateTools() => new(_queryDispatcher.Object, Mock.Of<ICommandDispatcher>());

    /// <summary>
    /// Given criteria that say nothing about paging
    /// When get-bills is called
    /// Then the query should ask for the first page of twenty
    /// </summary>
    /// <remarks>
    /// The query itself makes paging required, so something has to supply it. If that were left to
    /// the caller, an LLM asking a simple question about a bill would have to pick a page size
    /// first, and a zero page size returns nothing at all.
    /// </remarks>
    [Fact]
    public async Task GetBills_NoPagingSupplied_UsesTheDefaultFirstPage()
    {
        // Arrange
        var tools = CreateTools();

        // Act
        await tools.GetBills(new BillQuery(), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_dispatched);
        Assert.Equal(20, _dispatched.PageSize);
        Assert.Equal(1, _dispatched.PageNumber);
    }

    /// <summary>
    /// Given criteria that do say how to page
    /// When get-bills is called
    /// Then the query should use what was asked for rather than the defaults
    /// </summary>
    [Fact]
    public async Task GetBills_PagingSupplied_UsesTheSuppliedPaging()
    {
        // Arrange
        var tools = CreateTools();

        // Act
        await tools.GetBills(new BillQuery { PageSize = 5, PageNumber = 3 }, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_dispatched);
        Assert.Equal(5, _dispatched.PageSize);
        Assert.Equal(3, _dispatched.PageNumber);
    }

    /// <summary>
    /// Given criteria carrying every filter the tool offers
    /// When get-bills is called
    /// Then each filter should reach the query unchanged
    /// </summary>
    /// <remarks>
    /// A filter dropped in the mapping does not fail, it widens the answer: the caller asks about
    /// one account's gas bills and is told about all of them.
    /// </remarks>
    [Fact]
    public async Task GetBills_FiltersSupplied_PassesThemToTheQuery()
    {
        // Arrange
        var tools = CreateTools();
        var accountId = Guid.NewGuid();
        var criteria = new BillQuery
        {
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountId = accountId,
            UtilityType = UtilityType.Gas,
        };

        // Act
        await tools.GetBills(criteria, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_dispatched);
        Assert.Equal(new DateOnly(2024, 1, 1), _dispatched.StartDate);
        Assert.Equal(new DateOnly(2024, 12, 31), _dispatched.EndDate);
        Assert.Equal(accountId, _dispatched.AccountId);
        Assert.Equal(UtilityType.Gas, _dispatched.UtilityType);
    }

    /// <summary>
    /// Given criteria with no filters set
    /// When get-bills is called
    /// Then the query should carry no filters either
    /// </summary>
    /// <remarks>
    /// Guards against a mapping that invents a bound of its own, such as a default date range,
    /// which would hide older bills from a caller that asked for everything.
    /// </remarks>
    [Fact]
    public async Task GetBills_NoFiltersSupplied_DispatchesAnUnfilteredQuery()
    {
        // Arrange
        var tools = CreateTools();

        // Act
        await tools.GetBills(new BillQuery(), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_dispatched);
        Assert.Null(_dispatched.StartDate);
        Assert.Null(_dispatched.EndDate);
        Assert.Null(_dispatched.AccountId);
        Assert.Null(_dispatched.UtilityType);
    }
}
