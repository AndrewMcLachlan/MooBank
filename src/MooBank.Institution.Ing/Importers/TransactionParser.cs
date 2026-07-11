using System.Globalization;
using System.Text.RegularExpressions;
using Asm.MooBank.Institution.Ing.Models;

namespace Asm.MooBank.Institution.Ing.Importers;

internal partial class TransactionParser
{
    private static readonly string[] PurchaseDateFormats = ["dd MMM yyyy", "dd/MM/yyyy", "yyyy-MM-dd"];
    private const string PurchaseDateTimeFormat = "dd MMM yyyy h:mmtt";

    [GeneratedRegex("^(.+) - Visa (?:Purchase|Refund|Purchase Correction) - Receipt (\\d{1,6}) *In (.*) Date (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex VisaPurchaseRefundCorrection();

    [GeneratedRegex("^(.+) - Visa (?:Purchase|Cash Withdrawal) - Receipt (\\d{1,6})Foreign Currency Amount:(.+)In (.*) Date (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex VisaForeignCurrency();

    [GeneratedRegex("^(.+) - Direct Debit - Receipt (\\d{1,6}) (.+)")]
    private static partial Regex DirectDebit();

    [GeneratedRegex("^(.+) - Internal Transfer - Receipt (\\d{1,6}) (.*)")]
    private static partial Regex InternalTransfer();

    [GeneratedRegex("^(.+) - EFTPOS (?:Purchase|Refund|Purchase Correction) - Receipt (\\d{1,6})Date (.+) Time (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex EftposPurchaseRefund();

    [GeneratedRegex("^^(.+) - EFTPOS Purchase with Cash Out - Receipt (\\d{1,6}) - Cash amount \\$(.*)<BR/>Date (.+) Time (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex EftposCashOut();

    [GeneratedRegex("^([^-]+) - Receipt (\\d{1,6})")]
    private static partial Regex DirectPayment();

    [GeneratedRegex("^(.+?)\\s*-\\s*\\1 - Receipt (\\d{1,6})In (.*) Date (.+) Time (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex DirectPayment2();

    [GeneratedRegex("^(.+) - Receipt (\\d{1,6})In (.*) Date (.+) Time (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex DirectPayment3();

    [GeneratedRegex("^(.+) - Osko Payment( to)?(.*) - Receipt (\\d{1,6})(  - Ref )?(.+)?")]
    private static partial Regex OskoPayment();

    [GeneratedRegex("^Salary - Salary Deposit - Receipt (\\d{1,6})(.+)")]
    private static partial Regex SalaryDeposit();

    [GeneratedRegex("^(.+) - BPAY Bill Payment - Receipt (\\d{1,6}) To (.+)")]
    private static partial Regex Bpay();

    [GeneratedRegex("^(.+) - Receipt (\\d{1,6})ATM owner fee of \\$(.*) charged by .*Date (.+) Time (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex Atm();

    internal static ParsedTransaction ParseDescription(string? description)
    {
        ParsedTransaction parsed = new();

        description = description?.Trim('"') ?? String.Empty;

        var match = VisaPurchaseRefundCorrection().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.Location = match.Groups[3].Value.Trim();
            parsed.PurchaseDate = ParsePurchaseDate(match.Groups[4].Value);
            parsed.PurchaseType = "Visa";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Last4Digits = Int16.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
            parsed.TransactionSubType = MooBank.TransactionSubType.Visa;
            return parsed;
        }

        match = VisaForeignCurrency().Match(description);

        if (match.Success)
        {
            parsed.Description = $"{match.Groups[1].Value.Trim()} ({match.Groups[3].Value.Trim()})";
            parsed.Location = match.Groups[4].Value.Trim();
            parsed.PurchaseDate = ParsePurchaseDate(match.Groups[5].Value);
            parsed.PurchaseType = "Visa";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Last4Digits = Int16.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
            parsed.TransactionSubType = MooBank.TransactionSubType.Visa;
            return parsed;
        }

        match = DirectDebit().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseType = "Direct Debit";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Reference = match.Groups[3].Value.Trim();
            parsed.TransactionSubType = MooBank.TransactionSubType.DirectDebit;
            return parsed;
        }

        match = InternalTransfer().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseType = "Internal Transfer";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Reference = match.Groups[3].Value.Trim();
            parsed.TransactionSubType = MooBank.TransactionSubType.Transfer;
            return parsed;
        }

        match = EftposPurchaseRefund().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseDate = ParsePurchaseDateTime(match.Groups[3].Value, match.Groups[4].Value);
            parsed.PurchaseType = "EFTPOS";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Last4Digits = Int16.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
            parsed.TransactionSubType = MooBank.TransactionSubType.Eftpos;
            return parsed;
        }

        match = EftposCashOut().Match(description);
        if (match.Success)
        {
            parsed.Description = $"{match.Groups[1].Value.Trim()} - ${match.Groups[3]} Cash out";
            parsed.PurchaseDate = ParsePurchaseDateTime(match.Groups[4].Value, match.Groups[5].Value);
            parsed.PurchaseType = "EFTPOS";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Last4Digits = Int16.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
            parsed.TransactionSubType = MooBank.TransactionSubType.Eftpos;
            return parsed;
        }

        // The more specific direct payment patterns must be tried before the general one,
        // otherwise location, purchase date and card digits are lost.
        match = DirectPayment2().Match(description);
        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.Location = match.Groups[3].Value.Trim();
            parsed.PurchaseDate = ParsePurchaseDateTime(match.Groups[4].Value, match.Groups[5].Value);
            parsed.PurchaseType = "Direct";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Last4Digits = Int16.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
            return parsed;
        }

        match = DirectPayment3().Match(description);
        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.Location = match.Groups[3].Value.Trim();
            parsed.PurchaseDate = ParsePurchaseDateTime(match.Groups[4].Value, match.Groups[5].Value);
            parsed.PurchaseType = "Direct";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Last4Digits = Int16.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
            return parsed;
        }

        match = DirectPayment().Match(description);
        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseType = "Direct";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            return parsed;
        }

        match = OskoPayment().Match(description);
        if (match.Success)
        {
            parsed.Description = $"{match.Groups[1].Value.Trim()} ({match.Groups[3].Value})";
            parsed.PurchaseType = "Osko";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
            parsed.Reference = match.Groups[6].Value.Trim();
            parsed.TransactionSubType = MooBank.TransactionSubType.Osko;
            return parsed;
        }

        match = SalaryDeposit().Match(description);
        if (match.Success)
        {
            parsed.PurchaseType = "Salary";
            parsed.Description = $"Salary - {match.Groups[2].Value.Trim()}";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            return parsed;
        }

        match = Bpay().Match(description);
        if (match.Success)
        {
            parsed.PurchaseType = "BPAY";
            parsed.Description = $"{match.Groups[1].Value.Trim()} - {match.Groups[3].Value.Trim()}";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.TransactionSubType = MooBank.TransactionSubType.Bpay;
            return parsed;
        }

        match = Atm().Match(description);
        if (match.Success)
        {
            parsed.Description = $"{match.Groups[1].Value.Trim()} - ${match.Groups[3].Value.Trim()} fee";
            parsed.PurchaseDate = ParsePurchaseDateTime(match.Groups[4].Value, match.Groups[5].Value);
            parsed.PurchaseType = "ATM";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            parsed.Last4Digits = Int16.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
            parsed.TransactionSubType = MooBank.TransactionSubType.Atm;
            return parsed;
        }

        parsed.Description = description;
        return parsed;
    }

    /// <summary>
    /// Parses a purchase date, returning <see langword="null"/> rather than throwing when the value is
    /// in an unexpected format, so that a single malformed description does not abort an import.
    /// </summary>
    private static DateTime? ParsePurchaseDate(string value) =>
        DateTime.TryParseExact(value.Trim(), PurchaseDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) ? result : null;

    /// <summary>
    /// Parses a purchase date and time, returning <see langword="null"/> rather than throwing when the
    /// value is in an unexpected format, so that a single malformed description does not abort an import.
    /// </summary>
    private static DateTime? ParsePurchaseDateTime(string date, string time) =>
        DateTime.TryParseExact($"{date.Trim()} {time.Trim()}", PurchaseDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) ? result : null;
}
