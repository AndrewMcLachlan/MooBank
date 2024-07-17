using Asm.MooBank.Tools.TransactionGenerator;

List<Transaction> TransactionTypes =
[
    new() { Debit = -200, Description = "Supermarket", Frequency = 7 },
    new() { Debit = -20, Description = "Supermarket", Frequency = 3 },
    new() { Debit = -50, Description = "Bottle Shop", Frequency = 14 },
    new() { Debit = -5, Description = "Coffee Shop", Frequency = 2, FixedAmount = true, },
    new() { Debit = -15, Description = "Coffee Shop", Frequency = 7 },
    new() { Debit = -70, Description = "Restaurant", Frequency = 14 },
    new() { Debit = -100, Description = "Clothes Shop", Frequency = 14 },
    new() { Debit = -30, Description = "Takeaway", Frequency = 14 },
    new() { Debit = -500, Description = "Electricity Bill", Frequency = 90 },
    new() { Debit = -400, Description = "Rates", Frequency = 90 },
    new() { Debit = -400, Description = "Water Bill", Frequency = 90 },
    new() { Debit = -50, Description = "Pharmacy", Frequency = 30 },
    new() { Debit = -90, Description = "Doctor", Frequency = 60 },
    new() { Credit = 30, Description = "Medicare Rebate", Frequency = 60 },
    new() { Debit = -100, Description = "Fuel", Frequency = 14 },
    new() { Debit = -50, Description = "Haircut", Frequency = 42, FixedFrequency = true, FixedAmount = true },
    new() { Debit = -60, Description = "General Shopping", Frequency = 7 },
    new() { Debit = -50, Description = "Gifts", Frequency = 90 },
    new() { Debit = -200, Description = "Dentist", Frequency = 90 },
    new() { Debit = -500, Description = "Car Service", Frequency = 180 },
];



decimal startingBalance = 5000;

DateTime startDate = args.Length > 0 ? DateTime.Parse(args[0]) : new(DateTime.Now.Date.Year - 10, 1, 1);

DateTime endDate = DateTime.Today;



List<Transaction> transactions = [];

decimal balance = startingBalance;
DateTime current = startDate;

while (current <= endDate)
{

    Random frequencyRng = new(Guid.NewGuid().GetHashCode());

    int frequency = frequencyRng.Next(1, 365);

    List<string> completedTransactions = [];

    foreach (var transaction in TransactionTypes)
    {
        if (completedTransactions.Contains(transaction.Description))
        {
            continue;
        }

        if (transaction.FixedFrequency && transaction.LastDate != null && transaction.LastDate.Value.AddDays(transaction.Frequency) <= current)
        {
            transactions.Add(GenerateTransaction(transaction, current, ref balance));
        }
        else if (frequency % transaction.Frequency == 0)
        {
            transactions.Add(GenerateTransaction(transaction, current, ref balance));
        }

        transaction.LastDate = current;
        completedTransactions.Add(transaction.Description);
    }

    // Salary
    if (current.Day == 1)
    {
        const int salary = 5100;
        balance += salary;
        transactions.Add(new Transaction { Date = current, Credit = salary, Description = "Salary", Balance = balance });
    }

    if (current.Day == 3)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -400, Description = "Health Insurance", FixedAmount = true, }, current, ref balance));
    }

        if (current.Day == 10)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -100, Description = "Gym", FixedAmount = true, }, current, ref balance));
    }
    if (current.Day == 20)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -20, Description = "Netflix", FixedAmount = true, }, current, ref balance));
    }

    if (current.Day == 28)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -20, Description = "Spotify", FixedAmount = true, }, current, ref balance));
        transactions.Add(GenerateTransaction(new() { Debit = -1000, Description = "Mortgage", FixedAmount = true, }, current, ref balance));

        if (balance > 5000)
        {
            transactions.Add(GenerateTransaction(new() { Debit = -1000, Description = "Transfer to savings", FixedAmount = true, }, current, ref balance));
        }
    }
    if (current.Day == 15)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -40, Description = "Mobile", FixedAmount = true, }, current, ref balance));
        transactions.Add(GenerateTransaction(new() { Debit = -70, Description = "Internet", FixedAmount = true }, current, ref balance));
    }

    current = current.AddDays(1);

    if (current.Day == 15 && current.Month == 3)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -765, Description = "Rego", FixedAmount = true, }, current, ref balance));
    }

    // Holiday every year
    if (current.Day == 10 && current.Month == 5)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -2000, Description = "Flights" }, current, ref balance));
    }

    if (current.Day == 12 && current.Month == 5)
    {
        transactions.Add(GenerateTransaction(new() { Debit = -1000, Description = "Hotel" }, current, ref balance));
    }
}


using var csv = File.CreateText(@".\Transactions.csv");
csv.WriteLine("Date,Description,Credit,Debit,Balance");
foreach (var t in transactions)
{
    Console.WriteLine($"{t.Date:yyyy-MM-dd}, {t.Description}, {t.Credit}, {t.Debit}, {t.Balance}");
    csv.WriteLine($"{t.Date:dd/MM/yyyy},{t.Description},{(t.Credit == 0 ? String.Empty : t.Credit)},{(t.Debit == 0 ? String.Empty : t.Debit)},{t.Balance}");
}

static Transaction GenerateTransaction(Transaction transactionType, DateTime date, ref decimal balance)
{
    Random amountRng = new(Guid.NewGuid().GetHashCode());

    Transaction transaction = (Transaction)transactionType.Clone();

    var range = Convert.ToInt32(transactionType.Amount * 0.1m);

    decimal transactionAmountModifier = transactionType.FixedAmount ? 0 : Convert.ToDecimal(amountRng.Next(-range, range) + Math.Round(amountRng.NextDouble(), 2));

    if (transaction.Debit.HasValue)
    {
        transaction.Debit += transactionAmountModifier;
        balance -= transaction.Debit.Value;
    }
    else if (transaction.Credit.HasValue)
    {
        transaction.Credit += transactionAmountModifier;
        balance += transaction.Credit.Value;
    }

    transaction.Date = date;
    transaction.Balance = balance;

    return transaction;
}
