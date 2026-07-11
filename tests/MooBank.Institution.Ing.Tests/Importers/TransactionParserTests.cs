using Asm.MooBank.Institution.Ing.Importers;

namespace Asm.MooBank.Institution.Ing.Tests.Importers;

/// <summary>
/// Unit tests for the ING <see cref="TransactionParser"/>.
/// Covers regex ordering (specific direct payment patterns before the general one) and
/// graceful handling of malformed dates.
/// </summary>
[Trait("Category", "Unit")]
public class TransactionParserTests
{
    /// <summary>
    /// Given a direct payment description with location, date, time and card details
    /// When the description is parsed
    /// Then the detailed pattern wins and the extra fields are captured (regression test
    /// for the general pattern shadowing the specific ones).
    /// </summary>
    [Fact]
    public void ParseDescription_DirectPaymentWithCardDetails_ParsesLocationDateAndCard()
    {
        var parsed = TransactionParser.ParseDescription(
            "EZI*STORE - Receipt 123456In SYDNEY Date 01 Jun 2024 Time 1:23PM Card 462263xxxxxx1234");

        Assert.Equal("EZI*STORE", parsed.Description);
        Assert.Equal("SYDNEY", parsed.Location);
        Assert.Equal(new DateTime(2024, 6, 1, 13, 23, 0), parsed.PurchaseDate);
        Assert.Equal(123456, parsed.ReceiptNumber);
        Assert.Equal((short)1234, parsed.Last4Digits);
        Assert.Equal("Direct", parsed.PurchaseType);
    }

    /// <summary>
    /// Given a direct payment description with a repeated name, location and card details
    /// When the description is parsed
    /// Then the repeated-name pattern wins and the name is not duplicated.
    /// </summary>
    [Fact]
    public void ParseDescription_DirectPaymentWithRepeatedName_ParsesSingleName()
    {
        var parsed = TransactionParser.ParseDescription(
            "STORE - STORE - Receipt 123456In SYDNEY Date 01 Jun 2024 Time 1:23PM Card 462263xxxxxx1234");

        Assert.Equal("STORE", parsed.Description);
        Assert.Equal("SYDNEY", parsed.Location);
        Assert.Equal(new DateTime(2024, 6, 1, 13, 23, 0), parsed.PurchaseDate);
        Assert.Equal(123456, parsed.ReceiptNumber);
        Assert.Equal((short)1234, parsed.Last4Digits);
    }

    /// <summary>
    /// Given a simple direct payment description without card details
    /// When the description is parsed
    /// Then the general pattern still matches.
    /// </summary>
    [Fact]
    public void ParseDescription_SimpleDirectPayment_StillMatches()
    {
        var parsed = TransactionParser.ParseDescription("EZISTORE - Receipt 123456");

        Assert.Equal("EZISTORE", parsed.Description);
        Assert.Equal(123456, parsed.ReceiptNumber);
        Assert.Equal("Direct", parsed.PurchaseType);
        Assert.Null(parsed.Location);
        Assert.Null(parsed.PurchaseDate);
    }

    /// <summary>
    /// Given a Visa purchase description with a well-formed date
    /// When the description is parsed
    /// Then the purchase date is captured.
    /// </summary>
    [Fact]
    public void ParseDescription_VisaPurchase_ParsesPurchaseDate()
    {
        var parsed = TransactionParser.ParseDescription(
            "Some Store - Visa Purchase - Receipt 123456 In SYDNEY Date 01 Jun 2024 Card 462263xxxxxx1234");

        Assert.Equal("Some Store", parsed.Description);
        Assert.Equal("SYDNEY", parsed.Location);
        Assert.Equal(new DateTime(2024, 6, 1), parsed.PurchaseDate);
        Assert.Equal(123456, parsed.ReceiptNumber);
        Assert.Equal((short)1234, parsed.Last4Digits);
        Assert.Equal(MooBank.TransactionSubType.Visa, parsed.TransactionSubType);
    }

    /// <summary>
    /// Given a Visa purchase description with a malformed date
    /// When the description is parsed
    /// Then no exception is thrown and the purchase date is simply omitted (regression test
    /// for the unguarded, culture-sensitive DateTime.Parse that aborted the whole import).
    /// </summary>
    [Fact]
    public void ParseDescription_VisaPurchaseWithMalformedDate_DoesNotThrow()
    {
        var parsed = TransactionParser.ParseDescription(
            "Some Store - Visa Purchase - Receipt 123456 In SYDNEY Date garbage Card 462263xxxxxx1234");

        Assert.Equal("Some Store", parsed.Description);
        Assert.Null(parsed.PurchaseDate);
        Assert.Equal(123456, parsed.ReceiptNumber);
        Assert.Equal(MooBank.TransactionSubType.Visa, parsed.TransactionSubType);
    }

    /// <summary>
    /// Given an EFTPOS purchase description
    /// When the description is parsed
    /// Then the purchase date and time are captured as before.
    /// </summary>
    [Fact]
    public void ParseDescription_EftposPurchase_ParsesPurchaseDate()
    {
        var parsed = TransactionParser.ParseDescription(
            "Some Store - EFTPOS Purchase - Receipt 123456Date 01 Jun 2024 Time 1:23PM Card 462263xxxxxx1234");

        Assert.Equal("Some Store", parsed.Description);
        Assert.Equal(new DateTime(2024, 6, 1, 13, 23, 0), parsed.PurchaseDate);
        Assert.Equal(123456, parsed.ReceiptNumber);
        Assert.Equal(MooBank.TransactionSubType.Eftpos, parsed.TransactionSubType);
    }

    /// <summary>
    /// Given a description that matches no pattern
    /// When the description is parsed
    /// Then the description is returned as-is with no receipt number.
    /// </summary>
    [Fact]
    public void ParseDescription_UnrecognisedDescription_ReturnsDescriptionAsIs()
    {
        var parsed = TransactionParser.ParseDescription("Mystery Payment");

        Assert.Equal("Mystery Payment", parsed.Description);
        Assert.Null(parsed.ReceiptNumber);
    }
}
