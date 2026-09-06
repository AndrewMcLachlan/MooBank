using Asm.MooBank.Modules.Retirement.Commands;
using Asm.MooBank.Modules.Retirement.Models;

namespace Asm.MooBank.Modules.Retirement.Tests.Commands;

[Trait("Category", "Unit")]
public class MemberValidatorTests
{
    private readonly RetirementPlanMemberValidator _validator = new();

    private static RetirementPlanMember Member(GrowthStrategy strategy, decimal? customReturnRate) =>
        new()
        {
            UserId = Guid.NewGuid(),
            CurrentAge = 47,
            CurrentIncome = 100_000m,
            RetirementAge = 67,
            GrowthStrategy = strategy,
            CustomReturnRate = customReturnRate,
        };

    /// <summary>
    /// Given a member on the custom strategy with no rate
    /// When they are validated
    /// Then it should fail
    /// </summary>
    /// <remarks>
    /// The database refuses the row, so without this the reader gets a constraint violation where a
    /// sentence would do.
    /// </remarks>
    [Fact]
    public void Validate_CustomWithoutARate_Fails()
    {
        var result = _validator.Validate(Member(GrowthStrategy.Custom, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "A custom strategy needs a return rate");
    }

    /// <summary>
    /// Given a member on a named strategy carrying a rate of their own
    /// When they are validated
    /// Then it should fail
    /// </summary>
    /// <remarks>
    /// The same mistake from the other side: the rate would be stored and never read, so the member
    /// would silently grow at the strategy's rate instead of the one on screen.
    /// </remarks>
    [Fact]
    public void Validate_NamedStrategyWithARate_Fails()
    {
        var result = _validator.Validate(Member(GrowthStrategy.Balanced, 0.09m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Only a custom strategy carries its own return rate");
    }

    [Theory]
    [InlineData(GrowthStrategy.Custom, 0.0625)]
    [InlineData(GrowthStrategy.Growth, null)]
    public void Validate_AStrategyAndItsRateAgreeing_Passes(GrowthStrategy strategy, double? rate)
    {
        var result = _validator.Validate(Member(strategy, (decimal?)rate));

        Assert.True(result.IsValid, String.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }
}
