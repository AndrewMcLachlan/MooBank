using Asm.MooBank.Models;

namespace Asm.MooBank.Core.Tests.Models;

/// <summary>
/// Unit tests for the <see cref="Quarter"/> value object.
/// Tests cover parsing, creation from dates, operators, and string conversion.
/// </summary>
public class QuarterTests
{
    #region Parse

    /// <summary>
    /// Given a valid quarter string "2024-Q1"
    /// When Quarter.Parse is called
    /// Then Year should be 2024 and QuarterNumber should be 1
    /// </summary>
    [Theory]
    [InlineData("2024-Q1", 2024, 1)]
    [InlineData("2024-Q2", 2024, 2)]
    [InlineData("2024-Q3", 2024, 3)]
    [InlineData("2024-Q4", 2024, 4)]
    [InlineData("2000-Q1", 2000, 1)]
    [Trait("Category", "Unit")]
    public void Parse_ValidQuarterString_ReturnsCorrectQuarter(string input, int expectedYear, int expectedQuarter)
    {
        // Act
        var quarter = Quarter.Parse(input);

        // Assert
        Assert.Equal(expectedYear, quarter.Year);
        Assert.Equal(expectedQuarter, quarter.QuarterNumber);
    }

    /// <summary>
    /// Given a string with invalid format
    /// When Quarter.Parse is called
    /// Then a FormatException should be thrown
    /// </summary>
    [Theory]
    [InlineData("invalid")]
    [InlineData("2024")]
    [InlineData("Q1")]
    [Trait("Category", "Unit")]
    public void Parse_InvalidQuarterString_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => Quarter.Parse(input));
    }

    /// <summary>
    /// Given a string with valid format but invalid quarter number
    /// When Quarter.Parse is called
    /// Then an ArgumentOutOfRangeException should be thrown
    /// </summary>
    [Theory]
    [InlineData("2024-Q0")]
    [InlineData("2024-Q5")]
    [Trait("Category", "Unit")]
    public void Parse_OutOfRangeQuarterNumber_ThrowsArgumentOutOfRangeException(string input)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Quarter.Parse(input));
    }

    #endregion

    #region TryParse

    /// <summary>
    /// Given a valid quarter string
    /// When Quarter.TryParse is called
    /// Then it should return true and output the correct quarter
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void TryParse_ValidQuarterString_ReturnsTrueWithQuarter()
    {
        // Act
        var result = Quarter.TryParse("2024-Q3", out var quarter);

        // Assert
        Assert.True(result);
        Assert.Equal(2024, quarter.Year);
        Assert.Equal(3, quarter.QuarterNumber);
    }

    /// <summary>
    /// Given an invalid quarter string
    /// When Quarter.TryParse is called
    /// Then it should return false
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void TryParse_InvalidQuarterString_ReturnsFalse()
    {
        // Act
        var result = Quarter.TryParse("not-a-quarter", out _);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Given null input
    /// When Quarter.TryParse is called
    /// Then it should return false
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void TryParse_NullInput_ReturnsFalse()
    {
        // Act
        var result = Quarter.TryParse(null, out _);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region FromDate

    /// <summary>
    /// Given a date in January
    /// When Quarter.FromDate is called
    /// Then QuarterNumber should be 1
    /// </summary>
    [Theory]
    [InlineData(1, 1)]  // January -> Q1
    [InlineData(2, 1)]  // February -> Q1
    [InlineData(3, 1)]  // March -> Q1
    [InlineData(4, 2)]  // April -> Q2
    [InlineData(5, 2)]  // May -> Q2
    [InlineData(6, 2)]  // June -> Q2
    [InlineData(7, 3)]  // July -> Q3
    [InlineData(8, 3)]  // August -> Q3
    [InlineData(9, 3)]  // September -> Q3
    [InlineData(10, 4)] // October -> Q4
    [InlineData(11, 4)] // November -> Q4
    [InlineData(12, 4)] // December -> Q4
    [Trait("Category", "Unit")]
    public void FromDate_ForMonth_ReturnsCorrectQuarter(int month, int expectedQuarter)
    {
        // Arrange
        var date = new DateTime(2024, month, 15);

        // Act
        var quarter = Quarter.FromDate(date);

        // Assert
        Assert.Equal(2024, quarter.Year);
        Assert.Equal(expectedQuarter, quarter.QuarterNumber);
    }

    #endregion

    #region Equality

    /// <summary>
    /// Given two quarters with the same year and quarter number
    /// When compared with == operator
    /// Then they should be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_SameYearAndQuarter_ReturnsTrue()
    {
        // Arrange
        var quarter1 = Quarter.Parse("2024-Q2");
        var quarter2 = Quarter.Parse("2024-Q2");

        // Assert
        Assert.Equal(quarter1, quarter2);
        Assert.True(quarter1 == quarter2);
        Assert.False(quarter1 != quarter2);
    }

    /// <summary>
    /// Given two quarters with different quarter numbers
    /// When compared with != operator
    /// Then they should not be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_DifferentQuarter_ReturnsFalse()
    {
        // Arrange
        var quarter1 = Quarter.Parse("2024-Q2");
        var quarter2 = Quarter.Parse("2024-Q3");

        // Assert
        Assert.NotEqual(quarter1, quarter2);
        Assert.True(quarter1 != quarter2);
        Assert.False(quarter1 == quarter2);
    }

    #endregion

    #region Comparison Operators

    /// <summary>
    /// Given Q1 and Q2 of the same year
    /// When compared with less than operator
    /// Then Q1 should be less than Q2
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void LessThan_EarlierQuarterSameYear_ReturnsTrue()
    {
        // Arrange
        var q1 = Quarter.Parse("2024-Q1");
        var q2 = Quarter.Parse("2024-Q2");

        // Assert
        Assert.True(q1 < q2);
        Assert.True(q1 <= q2);
        Assert.False(q1 > q2);
        Assert.False(q1 >= q2);
    }

    /// <summary>
    /// Given Q4 2023 and Q1 2024
    /// When compared with less than operator
    /// Then Q4 2023 should be less than Q1 2024
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void LessThan_EarlierYear_ReturnsTrue()
    {
        // Arrange
        var q2023 = Quarter.Parse("2023-Q4");
        var q2024 = Quarter.Parse("2024-Q1");

        // Assert
        Assert.True(q2023 < q2024);
    }

    /// <summary>
    /// Given Q3 and Q2 of the same year
    /// When compared with greater than operator
    /// Then Q3 should be greater than Q2
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GreaterThan_LaterQuarterSameYear_ReturnsTrue()
    {
        // Arrange
        var q3 = Quarter.Parse("2024-Q3");
        var q2 = Quarter.Parse("2024-Q2");

        // Assert
        Assert.True(q3 > q2);
        Assert.True(q3 >= q2);
    }

    #endregion

    #region ToString

    /// <summary>
    /// Given a Quarter with year 2024 and quarter 2
    /// When ToString is called
    /// Then it should return "2024-Q2"
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var quarter = Quarter.Parse("2024-Q2");

        // Act
        var result = quarter.ToString();

        // Assert
        Assert.Equal("2024-Q2", result);
    }

    #endregion

    #region Constructor Validation

    /// <summary>
    /// Given an invalid quarter number (5)
    /// When Quarter constructor is called
    /// Then ArgumentOutOfRangeException should be thrown
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    [Trait("Category", "Unit")]
    public void Constructor_InvalidQuarterNumber_ThrowsArgumentOutOfRangeException(int quarterNumber)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Quarter(2024, quarterNumber));
    }

    #endregion

    #region Implicit Operators

    /// <summary>
    /// Given a Quarter value
    /// When implicitly converted to string
    /// Then it should return the formatted string
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ImplicitToString_ReturnsFormattedQuarter()
    {
        // Arrange
        Quarter quarter = new(2024, 3);

        // Act
        string result = quarter;

        // Assert
        Assert.Equal("2024-Q3", result);
    }

    /// <summary>
    /// Given a valid quarter string
    /// When implicitly converted to Quarter
    /// Then it should parse correctly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ImplicitFromString_ParsesQuarterCorrectly()
    {
        // Arrange
        string quarterStr = "2024-Q2";

        // Act
        Quarter quarter = quarterStr;

        // Assert
        Assert.Equal(2024, quarter.Year);
        Assert.Equal(2, quarter.QuarterNumber);
    }

    #endregion

    #region GetHashCode

    /// <summary>
    /// Given two equal quarters
    /// When GetHashCode is called on both
    /// Then they should have the same hash code
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetHashCode_EqualQuarters_SameHashCode()
    {
        // Arrange
        var quarter1 = new Quarter(2024, 2);
        var quarter2 = new Quarter(2024, 2);

        // Assert
        Assert.Equal(quarter1.GetHashCode(), quarter2.GetHashCode());
    }

    /// <summary>
    /// Given two different quarters
    /// When GetHashCode is called on both
    /// Then they should have different hash codes
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetHashCode_DifferentQuarters_DifferentHashCode()
    {
        // Arrange
        var quarter1 = new Quarter(2024, 1);
        var quarter2 = new Quarter(2024, 4);

        // Assert
        Assert.NotEqual(quarter1.GetHashCode(), quarter2.GetHashCode());
    }

    #endregion

    #region CompareTo

    /// <summary>
    /// Given a Quarter compared to null
    /// When CompareTo is called
    /// Then it should return positive (Quarter is greater than null)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var quarter = new Quarter(2024, 1);

        // Act
        var result = ((IComparable)quarter).CompareTo(null);

        // Assert
        Assert.True(result > 0);
    }

    /// <summary>
    /// Given a Quarter compared to a non-Quarter object
    /// When CompareTo is called
    /// Then an ArgumentException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void CompareTo_WithNonQuarter_ThrowsArgumentException()
    {
        // Arrange
        var quarter = new Quarter(2024, 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ((IComparable)quarter).CompareTo("not a quarter"));
    }

    /// <summary>
    /// Given two equal quarters
    /// When CompareTo is called
    /// Then it should return 0
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void CompareTo_EqualQuarters_ReturnsZero()
    {
        // Arrange
        var quarter1 = new Quarter(2024, 2);
        var quarter2 = new Quarter(2024, 2);

        // Act
        var result = quarter1.CompareTo(quarter2);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region FromDate with StartMonth

    /// <summary>
    /// Given a date in February with fiscal year starting in July
    /// When FromDate is called
    /// Then QuarterNumber should be 3 (Feb is in Q3 for July-June fiscal)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void FromDate_WithCustomStartMonth_CalculatesCorrectQuarter()
    {
        // Arrange - February 2024 with fiscal year starting July
        var date = new DateTime(2024, 2, 15);

        // Act
        var quarter = Quarter.FromDate(date, 7);

        // Assert
        Assert.Equal(2023, quarter.Year); // Fiscal year 2023-24
        Assert.Equal(3, quarter.QuarterNumber); // Feb is in Q3 for July start
    }

    /// <summary>
    /// Given a DateOnly value
    /// When FromDate is called
    /// Then it should return the correct quarter
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void FromDate_WithDateOnly_ReturnsCorrectQuarter()
    {
        // Arrange
        var date = new DateOnly(2024, 5, 15);

        // Act
        var quarter = Quarter.FromDate(date);

        // Assert
        Assert.Equal(2024, quarter.Year);
        Assert.Equal(2, quarter.QuarterNumber);
    }

    /// <summary>
    /// Given February 29 in a leap year
    /// When FromDate is called
    /// Then QuarterNumber should be 1 (Q1 for calendar year)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void FromDate_LeapYearFebruary_ReturnsCorrectQuarter()
    {
        // Arrange - Feb 29, 2024 is a leap year date
        var date = new DateTime(2024, 2, 29);

        // Act
        var quarter = Quarter.FromDate(date);

        // Assert
        Assert.Equal(2024, quarter.Year);
        Assert.Equal(1, quarter.QuarterNumber); // Feb is Q1 for calendar year
    }

    /// <summary>
    /// Given all possible fiscal year start months
    /// When FromDate is called for January 15
    /// Then the correct quarter should be returned for each
    /// </summary>
    [Theory]
    [InlineData(1, 2024, 1)]  // Calendar year: Jan = Q1
    [InlineData(2, 2023, 4)]  // Feb start: Jan = Q4 of previous year
    [InlineData(3, 2023, 4)]  // Mar start: Jan = Q4 of previous year
    [InlineData(4, 2023, 4)]  // Apr start: Jan = Q4 of previous year
    [InlineData(5, 2023, 3)]  // May start: Jan = Q3 of previous year
    [InlineData(6, 2023, 3)]  // Jun start: Jan = Q3 of previous year
    [InlineData(7, 2023, 3)]  // Jul start: Jan = Q3 of previous year
    [InlineData(8, 2023, 2)]  // Aug start: Jan = Q2 of previous year
    [InlineData(9, 2023, 2)]  // Sep start: Jan = Q2 of previous year
    [InlineData(10, 2023, 2)] // Oct start: Jan = Q2 of previous year
    [InlineData(11, 2023, 1)] // Nov start: Jan = Q1 of previous year
    [InlineData(12, 2023, 1)] // Dec start: Jan = Q1 of previous year
    [Trait("Category", "Unit")]
    public void FromDate_AllFiscalYearStarts_ReturnsCorrectQuarter(int startMonth, int expectedYear, int expectedQuarter)
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);

        // Act
        var quarter = Quarter.FromDate(date, startMonth);

        // Assert
        Assert.Equal(expectedYear, quarter.Year);
        Assert.Equal(expectedQuarter, quarter.QuarterNumber);
    }

    #endregion

    #region Comparison Operators - Compound Conditions

    /// <summary>
    /// Given quarters that require both year and quarter comparison
    /// When compared with less than operator
    /// Then the compound condition should evaluate correctly
    /// </summary>
    [Theory]
    [InlineData(2023, 4, 2024, 1, true)]   // Q4 2023 < Q1 2024
    [InlineData(2024, 1, 2023, 4, false)]  // Q1 2024 is NOT < Q4 2023
    [InlineData(2024, 2, 2024, 2, false)]  // Same quarter is NOT < itself
    [InlineData(2024, 1, 2024, 4, true)]   // Q1 < Q4 same year
    [Trait("Category", "Unit")]
    public void LessThan_CompoundCondition_EvaluatesCorrectly(int year1, int q1, int year2, int q2, bool expected)
    {
        // Arrange
        var quarter1 = new Quarter(year1, q1);
        var quarter2 = new Quarter(year2, q2);

        // Act & Assert
        Assert.Equal(expected, quarter1 < quarter2);
    }

    /// <summary>
    /// Given quarters that require both year and quarter comparison
    /// When compared with greater than operator
    /// Then the compound condition should evaluate correctly
    /// </summary>
    [Theory]
    [InlineData(2024, 1, 2023, 4, true)]   // Q1 2024 > Q4 2023
    [InlineData(2023, 4, 2024, 1, false)]  // Q4 2023 is NOT > Q1 2024
    [InlineData(2024, 2, 2024, 2, false)]  // Same quarter is NOT > itself
    [InlineData(2024, 4, 2024, 1, true)]   // Q4 > Q1 same year
    [Trait("Category", "Unit")]
    public void GreaterThan_CompoundCondition_EvaluatesCorrectly(int year1, int q1, int year2, int q2, bool expected)
    {
        // Arrange
        var quarter1 = new Quarter(year1, q1);
        var quarter2 = new Quarter(year2, q2);

        // Act & Assert
        Assert.Equal(expected, quarter1 > quarter2);
    }

    #endregion

    #region LessThanOrEqual / GreaterThanOrEqual

    /// <summary>
    /// Given two equal quarters
    /// When compared with <= and >= operators
    /// Then both should return true
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void LessThanOrEqual_GreaterThanOrEqual_EqualQuarters_BothTrue()
    {
        // Arrange
        var q1 = new Quarter(2024, 2);
        var q2 = new Quarter(2024, 2);

        // Assert
        Assert.True(q1 <= q2);
        Assert.True(q1 >= q2);
    }

    #endregion
}
