using System.Globalization;
using System.Text.RegularExpressions;
using Asm.MooBank.Domain.Entities.Ing;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Services.Ing;
public partial class TransactionParser
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

    public static TransactionExtra? ParseDescription(Transaction transaction)
    {
        TransactionExtra? transactionExtra = new();
        ParseDescription(transaction, ref transactionExtra);

        return transactionExtra;
    }

    public static void ParseDescription(Transaction transaction, ref TransactionExtra? transactionExtra)
    {
        if (transactionExtra == null) return;

        var description = transaction.Description?.Trim('"') ?? String.Empty;

        var match = VisaPurchase().Match(description);

        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.Description = match.Groups[1].Value.Trim();
            transactionExtra.Location = match.Groups[3].Value.Trim();
            transactionExtra.PurchaseDate = DateTime.Parse(match.Groups[4].Value);
            transactionExtra.PurchaseType = "Visa";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            transactionExtra.Last4Digits = Int16.Parse(match.Groups[5].Value);
            return;
        }

        match = VisaRefund().Match(description);

        if (match.Success)
        {

            transactionExtra.Transaction = transaction;
            transactionExtra.Description = match.Groups[1].Value.Trim();
            transactionExtra.Location = match.Groups[3].Value.Trim();
            transactionExtra.PurchaseDate = DateTime.Parse(match.Groups[4].Value);
            transactionExtra.PurchaseType = "Visa";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            transactionExtra.Last4Digits = Int16.Parse(match.Groups[5].Value);
            return;
        }

        match = VisaForeignCurrency().Match(description);

        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.Description = $"{match.Groups[1].Value.Trim()} ({match.Groups[3].Value.Trim()})";
            transactionExtra.Location = match.Groups[4].Value.Trim();
            transactionExtra.PurchaseDate = DateTime.Parse(match.Groups[5].Value);
            transactionExtra.PurchaseType = "Visa";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            transactionExtra.Last4Digits = Int16.Parse(match.Groups[6].Value);
            return;
        }

        match = DirectDebit().Match(description);

        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.Description = match.Groups[1].Value.Trim();
            transactionExtra.PurchaseType = "Direct Debit";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            transactionExtra.Reference = match.Groups[3].Value.Trim();
            return;
        }

        match = InternalTransfer().Match(description);

        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.Description = match.Groups[1].Value.Trim();
            transactionExtra.PurchaseType = "Internal Transfer";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            transactionExtra.Reference = match.Groups[3].Value.Trim();
            return;
        }

        match = EftposPurchase().Match(description);

        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.Description = match.Groups[1].Value.Trim();
            transactionExtra.PurchaseDate = DateTime.ParseExact($"{match.Groups[3].Value} {match.Groups[4].Value}", "dd MMM yyyy h:mmtt", CultureInfo.InvariantCulture);
            transactionExtra.PurchaseType = "EFTPOS";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            transactionExtra.Last4Digits = Int16.Parse(match.Groups[5].Value);
            return;
        }

        match = DirectPayment().Match(description);
        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.Description = match.Groups[1].Value.Trim();
            transactionExtra.PurchaseType = "Direct";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            return;
        }

        match= OskoPayment().Match(description);
        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.Description = $"{match.Groups[1].Value.Trim()} ({match.Groups[3].Value})";
            transactionExtra.PurchaseType = "Osko";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[4].Value);
            transactionExtra.Reference = match.Groups[6].Value.Trim();
            return;
        }

        match = SalaryDeposit().Match(description);
        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.PurchaseType = "Salary";
            transactionExtra.Description = $"Salary - {match.Groups[2].Value.Trim()}";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[1].Value);
            return;
        }

        match = Bpay().Match(description);
        if (match.Success)
        {
            transactionExtra.Transaction = transaction;
            transactionExtra.PurchaseType = "BPAY";
            transactionExtra.Description = $"{match.Groups[1].Value.Trim()} - {match.Groups[3].Value.Trim()}";
            transactionExtra.ReceiptNumber = Int32.Parse(match.Groups[2].Value);
            return;
        }

        transactionExtra = null;
    }

}
