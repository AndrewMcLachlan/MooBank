using System.Globalization;

namespace Asm.MooBank.Tools.TransactionGenerator;

/// <summary>
/// Generates transaction descriptions in ING-compatible formats.
/// These formats are parsed by the ING importer to extract structured data.
/// </summary>
public static class DescriptionBuilder
{
    private static int _receiptCounter = 100000;
    private static readonly object _lock = new();

    private static int NextReceipt()
    {
        lock (_lock)
        {
            return _receiptCounter++;
        }
    }

    /// <summary>
    /// Generates a Visa purchase description.
    /// Format: {merchant} - Visa Purchase - Receipt {receipt} In {location} Date {date} Card 123456xxxxxx{last4}
    /// </summary>
    public static string VisaPurchase(string merchant, string location, DateTime date, string cardLast4 = "1234")
    {
        var dateStr = date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        return $"{merchant} - Visa Purchase - Receipt {NextReceipt()} In {location} Date {dateStr} Card 123456xxxxxx{cardLast4}";
    }

    /// <summary>
    /// Generates a Visa refund description.
    /// Format: {merchant} - Visa Refund - Receipt {receipt} In {location} Date {date} Card 123456xxxxxx{last4}
    /// </summary>
    public static string VisaRefund(string merchant, string location, DateTime date, string cardLast4 = "1234")
    {
        var dateStr = date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        return $"{merchant} - Visa Refund - Receipt {NextReceipt()} In {location} Date {dateStr} Card 123456xxxxxx{cardLast4}";
    }

    /// <summary>
    /// Generates an EFTPOS purchase description.
    /// Format: {merchant} - EFTPOS Purchase - Receipt {receipt}Date {date} Time {time} Card 123456xxxxxx{last4}
    /// </summary>
    public static string EftposPurchase(string merchant, DateTime dateTime, string cardLast4 = "1234")
    {
        var dateStr = dateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var timeStr = dateTime.ToString("h:mmtt", CultureInfo.InvariantCulture);
        return $"{merchant} - EFTPOS Purchase - Receipt {NextReceipt()}Date {dateStr} Time {timeStr} Card 123456xxxxxx{cardLast4}";
    }

    /// <summary>
    /// Generates an EFTPOS refund description.
    /// Format: {merchant} - EFTPOS Refund - Receipt {receipt}Date {date} Time {time} Card 123456xxxxxx{last4}
    /// </summary>
    public static string EftposRefund(string merchant, DateTime dateTime, string cardLast4 = "1234")
    {
        var dateStr = dateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var timeStr = dateTime.ToString("h:mmtt", CultureInfo.InvariantCulture);
        return $"{merchant} - EFTPOS Refund - Receipt {NextReceipt()}Date {dateStr} Time {timeStr} Card 123456xxxxxx{cardLast4}";
    }

    /// <summary>
    /// Generates a Direct Debit description.
    /// Format: {payee} - Direct Debit - Receipt {receipt} {reference}
    /// </summary>
    public static string DirectDebit(string payee, string reference)
    {
        return $"{payee} - Direct Debit - Receipt {NextReceipt()} {reference}";
    }

    /// <summary>
    /// Generates an Internal Transfer description.
    /// Format: {description} - Internal Transfer - Receipt {receipt} {reference}
    /// </summary>
    public static string InternalTransfer(string description, string reference = "")
    {
        return $"{description} - Internal Transfer - Receipt {NextReceipt()} {reference}";
    }

    /// <summary>
    /// Generates an Osko Payment description.
    /// Format: {description} - Osko Payment to {recipient} - Receipt {receipt}  - Ref {reference}
    /// </summary>
    public static string OskoPayment(string description, string recipient, string reference)
    {
        return $"{description} - Osko Payment to {recipient} - Receipt {NextReceipt()}  - Ref {reference}";
    }

    /// <summary>
    /// Generates a Salary Deposit description.
    /// Format: Salary - Salary Deposit - Receipt {receipt}{employer}
    /// </summary>
    public static string SalaryDeposit(string employer)
    {
        return $"Salary - Salary Deposit - Receipt {NextReceipt()}{employer}";
    }

    /// <summary>
    /// Generates a BPAY Bill Payment description.
    /// Format: {biller} - BPAY Bill Payment - Receipt {receipt} To {billerCode}
    /// </summary>
    public static string BpayPayment(string biller, string billerCode)
    {
        return $"{biller} - BPAY Bill Payment - Receipt {NextReceipt()} To {billerCode}";
    }

    /// <summary>
    /// Generates an ATM withdrawal description.
    /// Format: ATM Withdrawal - Receipt {receipt}ATM owner fee of ${fee} charged by {bank}Date {date} Time {time} Card 123456xxxxxx{last4}
    /// </summary>
    public static string AtmWithdrawal(DateTime dateTime, decimal fee = 0, string bank = "ATM", string cardLast4 = "1234")
    {
        var dateStr = dateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var timeStr = dateTime.ToString("h:mmtt", CultureInfo.InvariantCulture);
        var feeStr = fee.ToString("F2", CultureInfo.InvariantCulture);
        return $"ATM Withdrawal - Receipt {NextReceipt()}ATM owner fee of ${feeStr} charged by {bank}Date {dateStr} Time {timeStr} Card 123456xxxxxx{cardLast4}";
    }

    /// <summary>
    /// Generates a simple direct payment description (fallback for unstructured payments).
    /// Format: {description} - Receipt {receipt}
    /// </summary>
    public static string DirectPayment(string description)
    {
        return $"{description} - Receipt {NextReceipt()}";
    }

    /// <summary>
    /// Generates a simple description for credits that don't match other patterns.
    /// </summary>
    public static string SimpleCredit(string description)
    {
        return description;
    }

    /// <summary>
    /// Generates a Medicare rebate description.
    /// </summary>
    public static string MedicareRebate()
    {
        return $"MEDICARE AUSTRALIA - Receipt {NextReceipt()}";
    }

    /// <summary>
    /// Generates a tax refund description.
    /// </summary>
    public static string TaxRefund()
    {
        return $"ATO TAX REFUND - Receipt {NextReceipt()}";
    }

    /// <summary>
    /// Generates a bonus/additional pay description.
    /// </summary>
    public static string BonusPayment(string employer)
    {
        return $"Salary - Salary Deposit - Receipt {NextReceipt()}{employer} - BONUS";
    }

    /// <summary>
    /// Generates an interest credit description.
    /// </summary>
    public static string InterestCredit()
    {
        return "Interest Credit";
    }

    /// <summary>
    /// Resets the receipt counter (useful for testing).
    /// </summary>
    public static void ResetReceiptCounter(int startValue = 100000)
    {
        lock (_lock)
        {
            _receiptCounter = startValue;
        }
    }
}
