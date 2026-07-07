using Asm.MooBank.Models;

namespace Asm.MooBank.Core.Tests.Models;

/// <summary>
/// Unit tests for the <see cref="StockSymbol"/> model.
/// Tests cover parsing, equality, and string conversions.
/// </summary>
public class StockSymbolTests
{
    #region Parse

    /// <summary>
    /// Given a simple symbol without exchange
    /// When Parse is called
    /// Then a StockSymbol with null exchange should be returned
    /// </summary>
    [Theory]
    [InlineData("AAPL")]
    [InlineData("GOOGL")]
    [InlineData("msft")]
    [Trait("Category", "Unit")]
    public void Parse_SimpleSymbol_ReturnsSymbolWithNullExchange(string input)
    {
        // Act
        var result = StockSymbol.Parse(input);

        // Assert
        Assert.Equal(input.ToUpperInvariant(), result.Symbol);
        Assert.Null(result.Exchange);
    }

    /// <summary>
    /// Given a symbol with exchange
    /// When Parse is called
    /// Then a StockSymbol with the exchange should be returned
    /// </summary>
    [Theory]
    [InlineData("AAPL.US", "AAPL", "US")]
    [InlineData("BHP.AU", "BHP", "AU")]
    [InlineData("vol.uk", "VOL", "UK")]
    [Trait("Category", "Unit")]
    public void Parse_SymbolWithExchange_ReturnsSymbolWithExchange(string input, string expectedSymbol, string expectedExchange)
    {
        // Act
        var result = StockSymbol.Parse(input);

        // Assert
        Assert.Equal(expectedSymbol, result.Symbol);
        Assert.Equal(expectedExchange, result.Exchange);
    }

    /// <summary>
    /// Given an invalid symbol format (too many parts)
    /// When Parse is called
    /// Then FormatException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_TooManyParts_ThrowsFormatException()
    {
        // Arrange
        var input = "AAPL.US.EXTRA";

        // Act & Assert
        Assert.Throws<FormatException>(() => StockSymbol.Parse(input));
    }

    /// <summary>
    /// Given a symbol with invalid exchange length
    /// When Parse is called
    /// Then FormatException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Parse_InvalidExchangeLength_ThrowsFormatException()
    {
        // Arrange
        var input = "AAPL.USA"; // Exchange should be 2 chars

        // Act & Assert
        Assert.Throws<FormatException>(() => StockSymbol.Parse(input));
    }

    /// <summary>
    /// Given a symbol containing invalid characters
    /// When Parse is called
    /// Then FormatException should be thrown
    /// </summary>
    [Theory]
    [InlineData("AA PL")]
    [InlineData("AA-PL")]
    [InlineData("")]
    [Trait("Category", "Unit")]
    public void Parse_InvalidCharacters_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => StockSymbol.Parse(input));
    }

    #endregion

    #region TryParse

    /// <summary>
    /// Given a simple symbol without an exchange suffix
    /// When TryParse is called
    /// Then true should be returned with a null exchange
    /// </summary>
    [Theory]
    [InlineData("AAPL")]
    [InlineData("msft")]
    [Trait("Category", "Unit")]
    public void TryParse_SymbolWithoutExchange_ReturnsTrue(string input)
    {
        // Act
        var result = StockSymbol.TryParse(input, out var symbol);

        // Assert
        Assert.True(result);
        Assert.NotNull(symbol);
        Assert.Equal(input.ToUpperInvariant(), symbol.Symbol);
        Assert.Null(symbol.Exchange);
    }

    /// <summary>
    /// Given a symbol with an exchange suffix
    /// When TryParse is called
    /// Then true should be returned with the exchange populated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void TryParse_SymbolWithExchange_ReturnsTrue()
    {
        // Act
        var result = StockSymbol.TryParse("bhp.au", out var symbol);

        // Assert
        Assert.True(result);
        Assert.NotNull(symbol);
        Assert.Equal("BHP", symbol.Symbol);
        Assert.Equal("AU", symbol.Exchange);
    }

    /// <summary>
    /// Given invalid input
    /// When TryParse is called
    /// Then false should be returned without throwing
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AAPL.US.EXTRA")]
    [InlineData("AAPL.USA")]
    [InlineData("AA PL")]
    [Trait("Category", "Unit")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        // Act
        var result = StockSymbol.TryParse(input, out var symbol);

        // Assert
        Assert.False(result);
        Assert.Null(symbol);
    }

    #endregion

    #region ToString

    /// <summary>
    /// Given a symbol with exchange
    /// When ToString is called
    /// Then the symbol.exchange format should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ToString_WithExchange_ReturnsFormattedString()
    {
        // Arrange
        var symbol = new StockSymbol("AAPL", "US");

        // Act
        var result = symbol.ToString();

        // Assert
        Assert.Equal("AAPL.US", result);
    }

    /// <summary>
    /// Given a symbol without exchange
    /// When ToString is called
    /// Then just the symbol should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ToString_WithoutExchange_ReturnsJustSymbol()
    {
        // Arrange
        var symbol = new StockSymbol("AAPL", null);

        // Act
        var result = symbol.ToString();

        // Assert
        Assert.Equal("AAPL", result);
    }

    #endregion

    #region Equality

    /// <summary>
    /// Given two symbols with same values
    /// When Equals is called
    /// Then true should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var symbol1 = new StockSymbol("AAPL", "US");
        var symbol2 = new StockSymbol("AAPL", "US");

        // Act & Assert
        Assert.True(symbol1.Equals(symbol2));
        Assert.True(symbol1 == symbol2);
    }

    /// <summary>
    /// Given two symbols with different symbols
    /// When Equals is called
    /// Then false should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_DifferentSymbol_ReturnsFalse()
    {
        // Arrange
        var symbol1 = new StockSymbol("AAPL", "US");
        var symbol2 = new StockSymbol("GOOGL", "US");

        // Act & Assert
        Assert.False(symbol1.Equals(symbol2));
        Assert.True(symbol1 != symbol2);
    }

    /// <summary>
    /// Given two symbols with different exchanges
    /// When Equals is called
    /// Then false should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_DifferentExchange_ReturnsFalse()
    {
        // Arrange
        var symbol1 = new StockSymbol("AAPL", "US");
        var symbol2 = new StockSymbol("AAPL", "AU");

        // Act & Assert
        Assert.False(symbol1.Equals(symbol2));
    }

    /// <summary>
    /// Given a symbol compared to null
    /// When Equals is called
    /// Then false should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var symbol = new StockSymbol("AAPL", "US");

        // Act & Assert
        Assert.False(symbol.Equals(null));
    }

    /// <summary>
    /// Given a symbol compared to an object of the same type
    /// When Equals(object) is called
    /// Then correct comparison should be made
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_ObjectOverload_ComparesCorrectly()
    {
        // Arrange
        var symbol1 = new StockSymbol("AAPL", "US");
        object symbol2 = new StockSymbol("AAPL", "US");

        // Act & Assert
        Assert.True(symbol1.Equals(symbol2));
    }

    /// <summary>
    /// Given null operands
    /// When the equality operators are used
    /// Then no exception should be thrown and the correct result returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void EqualityOperators_NullOperands_AreNullSafe()
    {
        // Arrange
        StockSymbol? nullSymbol = null;
        var symbol = new StockSymbol("AAPL", "US");

        // Act & Assert
        Assert.True(nullSymbol == null);
        Assert.False(nullSymbol == symbol);
        Assert.False(symbol == nullSymbol);
        Assert.True(nullSymbol != symbol);
        Assert.True(symbol != nullSymbol);
        Assert.False(nullSymbol != null);
    }

    #endregion

    #region GetHashCode

    /// <summary>
    /// Given two equal symbols
    /// When GetHashCode is called
    /// Then the hash codes should be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetHashCode_EqualSymbols_EqualHashCodes()
    {
        // Arrange
        var symbol1 = new StockSymbol("AAPL", "US");
        var symbol2 = new StockSymbol("AAPL", "US");

        // Act & Assert
        Assert.Equal(symbol1.GetHashCode(), symbol2.GetHashCode());
    }

    #endregion

    #region Implicit Conversions

    /// <summary>
    /// Given a string
    /// When implicitly converted to StockSymbol
    /// Then a valid StockSymbol should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ImplicitConversion_FromString_ReturnsStockSymbol()
    {
        // Act
        StockSymbol symbol = "AAPL.US";

        // Assert
        Assert.Equal("AAPL", symbol.Symbol);
        Assert.Equal("US", symbol.Exchange);
    }

    /// <summary>
    /// Given a StockSymbol
    /// When implicitly converted to string
    /// Then the formatted string should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ImplicitConversion_ToString_ReturnsFormattedString()
    {
        // Arrange
        var symbol = new StockSymbol("AAPL", "US");

        // Act
        string result = symbol;

        // Assert
        Assert.Equal("AAPL.US", result);
    }

    #endregion
}
