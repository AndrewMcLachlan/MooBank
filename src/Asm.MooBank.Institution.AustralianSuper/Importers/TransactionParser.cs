using System.Globalization;
using System.Text.RegularExpressions;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Institution.AustralianSuper.Models;

namespace Asm.MooBank.Institution.Ing.Importers;
internal partial class TransactionParser
{
    [GeneratedRegex("^(.+) - Visa Purchase - Receipt (\\d{4,6})In (.+) Date (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex VisaPurchase();

    [GeneratedRegex("^(.+) - Visa Refund - Receipt (\\d{4,6}) In (.+) Date (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex VisaRefund();

    [GeneratedRegex("^(.+) - Visa Purchase - Receipt (\\d{4,6})Foreign Currency Amount:(.+)In (.+) Date (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex VisaForeignCurrency();

    [GeneratedRegex("^(.+) - Direct Debit - Receipt (\\d{4,6}) (.+)")]
    private static partial Regex DirectDebit();

    [GeneratedRegex("^(.+) - Internal Transfer - Receipt (\\d{4,6}) (.*)")]
    private static partial Regex InternalTransfer();

    [GeneratedRegex("^(.+) - EFTPOS Purchase - Receipt (\\d{4,6})Date (.+) Time (.+) Card \\d{6}xxxxxx(\\d{4})")]
    private static partial Regex EftposPurchase();

    [GeneratedRegex("^([^-]+) - Receipt (\\d{4,6})")]
    private static partial Regex DirectPayment();

    [GeneratedRegex("^(.+) - Osko Payment( to)?(.*) - Receipt (\\d{4,6})(  - Ref )?(.+)?")]
    private static partial Regex OskoPayment();

    [GeneratedRegex("^Salary - Salary Deposit - Receipt (\\d{4,6})(.+)")]
    private static partial Regex SalaryDeposit();

    [GeneratedRegex("^(.+) - BPAY Bill Payment - Receipt (\\d{4,6}) To (.+)")]
    private static partial Regex Bpay();

    internal static ParsedTransaction ParseDescription(string? description)
    {
        ParsedTransaction parsed = new();

        description = description?.Trim('"') ?? String.Empty;

        var match = VisaPurchase().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.Location = match.Groups[3].Value.Trim();
            parsed.PurchaseDate = DateTime.Parse(match.Groups[4].Value);
            parsed.PurchaseType = "Visa";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            parsed.Last4Digits = Int16.Parse(match.Groups[5].Value);
            return parsed;
        }

        match = VisaRefund().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.Location = match.Groups[3].Value.Trim();
            parsed.PurchaseDate = DateTime.Parse(match.Groups[4].Value);
            parsed.PurchaseType = "Visa";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            parsed.Last4Digits = Int16.Parse(match.Groups[5].Value);
            return parsed;
        }

        match = VisaForeignCurrency().Match(description);

        if (match.Success)
        {
            parsed.Description = $"{match.Groups[1].Value.Trim()} ({match.Groups[3].Value.Trim()})";
            parsed.Location = match.Groups[4].Value.Trim();
            parsed.PurchaseDate = DateTime.Parse(match.Groups[5].Value);
            parsed.PurchaseType = "Visa";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            parsed.Last4Digits = Int16.Parse(match.Groups[6].Value);
            return parsed;
        }

        match = DirectDebit().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseType = "Direct Debit";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            parsed.Reference = match.Groups[3].Value.Trim();
            return parsed;
        }

        match = InternalTransfer().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseType = "Internal Transfer";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            parsed.Reference = match.Groups[3].Value.Trim();
            return parsed;
        }

        match = EftposPurchase().Match(description);

        if (match.Success)
        {
            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseDate = DateTime.ParseExact($"{match.Groups[3].Value} {match.Groups[4].Value}", "dd MMM yyyy h:mmtt", CultureInfo.InvariantCulture);
            parsed.PurchaseType = "EFTPOS";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            parsed.Last4Digits = Int16.Parse(match.Groups[5].Value);
            return parsed;
        }

        match = DirectPayment().Match(description);
        if (match.Success)
        {

            parsed.Description = match.Groups[1].Value.Trim();
            parsed.PurchaseType = "Direct";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            return parsed;
        }

        match= OskoPayment().Match(description);
        if (match.Success)
        {

            parsed.Description = $"{match.Groups[1].Value.Trim()} ({match.Groups[3].Value})";
            parsed.PurchaseType = "Osko";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[4].Value);
            parsed.Reference = match.Groups[6].Value.Trim();
            return parsed;
        }

        match = SalaryDeposit().Match(description);
        if (match.Success)
        {

            parsed.PurchaseType = "Salary";
            parsed.Description = $"Salary - {match.Groups[2].Value.Trim()}";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[1].Value);
            return parsed;
        }

        match = Bpay().Match(description);
        if (match.Success)
        {

            parsed.PurchaseType = "BPAY";
            parsed.Description = $"{match.Groups[1].Value.Trim()} - {match.Groups[3].Value.Trim()}";
            parsed.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            return parsed;
        }

        parsed.Description = description;
        return parsed;
    }
}
