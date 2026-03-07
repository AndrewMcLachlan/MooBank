namespace Asm.MooBank.Tools.TransactionGenerator;

/// <summary>
/// Generates realistic transaction data for a savings account.
/// Savings accounts typically have:
/// - Transfers in from transaction account
/// - Monthly interest credits
/// - Occasional withdrawals
/// </summary>
public class SavingsAccountGenerator
{
    private readonly Random _random = new();
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly List<(DateTime Date, decimal Amount)> _transfersIn;
    private decimal _balance;

    private readonly List<Transaction> _transactions = [];

    // Interest rate (annual) - typical Australian savings account
    private const decimal AnnualInterestRate = 0.045m; // 4.5% p.a.

    public SavingsAccountGenerator(
        decimal startingBalance,
        DateTime startDate,
        DateTime endDate,
        List<(DateTime Date, decimal Amount)> transfersIn)
    {
        _balance = startingBalance;
        _startDate = startDate;
        _endDate = endDate;
        _transfersIn = transfersIn;
    }

    public List<Transaction> Generate()
    {
        DescriptionBuilder.ResetReceiptCounter(200000); // Different receipt range from transaction account

        var currentDate = _startDate;
        var monthlyBalanceSum = 0m;
        var daysInMonth = 0;

        while (currentDate <= _endDate)
        {
            // Track daily balance for interest calculation
            monthlyBalanceSum += _balance;
            daysInMonth++;

            // Process transfers in from transaction account
            var transfersToday = _transfersIn.Where(t => t.Date == currentDate).ToList();
            foreach (var transfer in transfersToday)
            {
                GenerateTransferIn(currentDate, transfer.Amount);
            }

            // End of month - credit interest
            if (currentDate.Day == DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
            {
                var averageBalance = monthlyBalanceSum / daysInMonth;
                GenerateInterest(currentDate, averageBalance);

                // Reset for next month
                monthlyBalanceSum = 0m;
                daysInMonth = 0;
            }

            // Occasional withdrawal (about once every 2-3 months)
            if (currentDate.Day == 15 && _random.NextDouble() < 0.02 && _balance > 2000)
            {
                GenerateWithdrawal(currentDate);
            }

            currentDate = currentDate.AddDays(1);
        }

        return [.. _transactions.OrderBy(t => t.Date)];
    }

    private void GenerateTransferIn(DateTime date, decimal amount)
    {
        var description = DescriptionBuilder.OskoPayment("Transfer", "TRANSACTION ACCOUNT", $"SAV{_random.Next(10000, 99999)}");

        _balance += amount;
        _transactions.Add(new Transaction
        {
            Date = date,
            Description = description,
            Credit = amount,
            Balance = _balance,
            Category = "Transfer",
            Merchant = "TRANSACTION ACCOUNT",
            PaymentMethod = PaymentMethod.Osko
        });
    }

    private void GenerateInterest(DateTime date, decimal averageBalance)
    {
        // Monthly interest = (Annual Rate / 12) * Average Balance
        var monthlyRate = AnnualInterestRate / 12;
        var interest = Math.Round(averageBalance * monthlyRate, 2);

        if (interest <= 0) return;

        _balance += interest;
        _transactions.Add(new Transaction
        {
            Date = date,
            Description = DescriptionBuilder.InterestCredit(),
            Credit = interest,
            Balance = _balance,
            Category = "Interest",
            Merchant = "INTEREST",
            PaymentMethod = PaymentMethod.DirectCredit
        });
    }

    private void GenerateWithdrawal(DateTime date)
    {
        // Withdraw $500-2000 for a large purchase or emergency
        var amount = 500m + (decimal)(_random.NextDouble() * 1500);
        amount = Math.Min(amount, _balance - 1000); // Keep at least $1000 in savings

        if (amount < 100) return;

        amount = Math.Round(amount, 2);
        var description = DescriptionBuilder.OskoPayment("Transfer", "TRANSACTION ACCOUNT", $"TRF{_random.Next(10000, 99999)}");

        _balance -= amount;
        _transactions.Add(new Transaction
        {
            Date = date,
            Description = description,
            Debit = amount,
            Balance = _balance,
            Category = "Transfer",
            Merchant = "TRANSACTION ACCOUNT",
            PaymentMethod = PaymentMethod.Osko
        });
    }
}
