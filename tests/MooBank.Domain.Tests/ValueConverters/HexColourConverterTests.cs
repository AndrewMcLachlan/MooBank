#nullable enable
using Asm.Drawing;
using Asm.MooBank.Infrastructure.ValueConverters;

namespace Asm.MooBank.Domain.Tests.ValueConverters;

[Trait("Category", "Unit")]
public class HexColourConverterTests
{
    private readonly HexColourConverter _converter = new();

    #region ConvertToProvider (HexColour? -> string?)

    [Fact]
    public void ConvertToProvider_WithValidColour_ReturnsHexString()
    {
        // Arrange
        var colour = new HexColour("#FF5733");
        var convertToProvider = _converter.ConvertToProviderExpression.Compile();

        // Act
        var result = convertToProvider(colour);

        // Assert
        Assert.Equal("#FF5733", result);
    }

    [Fact]
    public void ConvertToProvider_WithNullColour_ReturnsNull()
    {
        // Arrange
        HexColour? colour = null;
        var convertToProvider = _converter.ConvertToProviderExpression.Compile();

        // Act
        var result = convertToProvider(colour);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region ConvertFromProvider (string? -> HexColour?)

    [Fact]
    public void ConvertFromProvider_WithValidHexString_ReturnsHexColour()
    {
        // Arrange
        var hexString = "#00FF00";
        var convertFromProvider = _converter.ConvertFromProviderExpression.Compile();

        // Act
        var result = convertFromProvider(hexString);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("#00FF00", result!.Value.HexString);
    }

    [Fact]
    public void ConvertFromProvider_WithNullString_ReturnsNull()
    {
        // Arrange
        string? hexString = null;
        var convertFromProvider = _converter.ConvertFromProviderExpression.Compile();

        // Act
        var result = convertFromProvider(hexString);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ConvertFromProvider_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var hexString = String.Empty;
        var convertFromProvider = _converter.ConvertFromProviderExpression.Compile();

        // Act
        var result = convertFromProvider(hexString);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Round-trip Conversion

    [Theory]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#ABCDEF")]
    [InlineData("#123456")]
    public void RoundTrip_ColourToStringAndBack_PreservesValue(string hexValue)
    {
        // Arrange
        var originalColour = new HexColour(hexValue);
        var convertToProvider = _converter.ConvertToProviderExpression.Compile();
        var convertFromProvider = _converter.ConvertFromProviderExpression.Compile();

        // Act
        var providerValue = convertToProvider(originalColour);
        var roundTrippedColour = convertFromProvider(providerValue);

        // Assert
        Assert.Equal(originalColour, roundTrippedColour);
    }

    [Fact]
    public void RoundTrip_NullToStringAndBack_PreservesNull()
    {
        // Arrange
        HexColour? originalColour = null;
        var convertToProvider = _converter.ConvertToProviderExpression.Compile();
        var convertFromProvider = _converter.ConvertFromProviderExpression.Compile();

        // Act
        var providerValue = convertToProvider(originalColour);
        var roundTrippedColour = convertFromProvider(providerValue);

        // Assert
        Assert.Null(roundTrippedColour);
    }

    #endregion
}
