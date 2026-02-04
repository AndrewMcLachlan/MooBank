#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Tests.Models;

[Trait("Category", "Unit")]
public class ReportTypeTests
{
    #region TryParse Tests

    [Fact]
    public void TryParse_CreditLowercase_ReturnsTrue()
    {
        // Act
        var result = ReportType.TryParse("credit", out var reportType);

        // Assert
        Assert.True(result);
        Assert.Equal(TransactionFilterType.Credit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void TryParse_CreditUppercase_ReturnsTrue()
    {
        // Act
        var result = ReportType.TryParse("CREDIT", out var reportType);

        // Assert
        Assert.True(result);
        Assert.Equal(TransactionFilterType.Credit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void TryParse_CreditMixedCase_ReturnsTrue()
    {
        // Act
        var result = ReportType.TryParse("CrEdIt", out var reportType);

        // Assert
        Assert.True(result);
        Assert.Equal(TransactionFilterType.Credit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void TryParse_DebitLowercase_ReturnsTrue()
    {
        // Act
        var result = ReportType.TryParse("debit", out var reportType);

        // Assert
        Assert.True(result);
        Assert.Equal(TransactionFilterType.Debit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void TryParse_DebitUppercase_ReturnsTrue()
    {
        // Act
        var result = ReportType.TryParse("DEBIT", out var reportType);

        // Assert
        Assert.True(result);
        Assert.Equal(TransactionFilterType.Debit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void TryParse_DebitMixedCase_ReturnsTrue()
    {
        // Act
        var result = ReportType.TryParse("DeBiT", out var reportType);

        // Assert
        Assert.True(result);
        Assert.Equal(TransactionFilterType.Debit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void TryParse_NullString_ReturnsFalse()
    {
        // Act
        var result = ReportType.TryParse(null!, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        // Act
        var result = ReportType.TryParse("", out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryParse_WhitespaceString_ReturnsFalse()
    {
        // Act
        var result = ReportType.TryParse("   ", out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryParse_InvalidString_ReturnsFalse()
    {
        // Act
        var result = ReportType.TryParse("invalid", out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryParse_PartialMatch_ReturnsFalse()
    {
        // Act
        var result = ReportType.TryParse("cred", out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryParse_InvalidString_OutputsDefaultValue()
    {
        // Act
        ReportType.TryParse("invalid", out var reportType);

        // Assert - default is Credit
        Assert.Equal(TransactionFilterType.Credit, (TransactionFilterType)reportType);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromCredit_ReturnsCorrectReportType()
    {
        // Arrange
        TransactionFilterType filterType = TransactionFilterType.Credit;

        // Act
        ReportType reportType = filterType;

        // Assert
        Assert.Equal(TransactionFilterType.Credit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void ImplicitConversion_FromDebit_ReturnsCorrectReportType()
    {
        // Arrange
        TransactionFilterType filterType = TransactionFilterType.Debit;

        // Act
        ReportType reportType = filterType;

        // Assert
        Assert.Equal(TransactionFilterType.Debit, (TransactionFilterType)reportType);
    }

    [Fact]
    public void ImplicitConversion_ToTransactionFilterType_ReturnsCorrectValue()
    {
        // Arrange
        ReportType.TryParse("debit", out var reportType);

        // Act
        TransactionFilterType filterType = reportType;

        // Assert
        Assert.Equal(TransactionFilterType.Debit, filterType);
    }

    [Fact]
    public void ImplicitConversion_InvalidFilterType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        TransactionFilterType filterType = (TransactionFilterType)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ReportType _ = filterType;
        });
        Assert.Equal("reportType", exception.ParamName);
    }

    [Fact]
    public void ImplicitConversion_RoundTrip_PreservesValue()
    {
        // Arrange
        TransactionFilterType original = TransactionFilterType.Credit;

        // Act - convert to ReportType and back
        ReportType reportType = original;
        TransactionFilterType roundTripped = reportType;

        // Assert
        Assert.Equal(original, roundTripped);
    }

    #endregion
}

// Note: ReportTypeExtensions tests require complex domain entity setup.
// The extension methods are tested implicitly through the GetByTagReport and
// GetInOutTrendReport tests which exercise these filters with proper entities.
