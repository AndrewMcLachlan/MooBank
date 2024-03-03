using Asm.MooBank.Tools.TransactionGenerator;

List<Transaction> TransactionTypes =
[
    new() { Debit = 200, Description = "Supermarket", Frequency = 7 },
    new() { Debit = 20, Description = "Supermarket", Frequency = 3 },
    new() { Debit = 50, Description = "Bottle Shop", Frequency = 14 },
    new() { Debit = 5, Description = "Coffee Shop", Frequency = 2 },
    new() { Debit = 70, Description = "Restaurant", Frequency = 14 },
    new() { Debit = 100, Description = "Clothes Shop", Frequency = 14 },
    new() { Debit = 30, Description = "Takeaway", Frequency = 14 },
    new() { Debit = 500, Description = "Electricity Bill", Frequency = 90 },
    new() { Debit = 400, Description = "Rates", Frequency = 90 },
    new() { Debit = 400, Description = "Water Bill", Frequency = 90 },
    new() { Debit = 50, Description = "Pharmacy", Frequency = 30 },
    new() { Debit = 90, Description = "Doctor", Frequency = 60 },
    new() { Credit = 30, Description = "Medicare Rebate", Frequency = 60 },
    new() { Debit = 100, Description = "Fuel", Frequency = 14 },
];



decimal startingBalance = 5000;

DateTime startDate = new(DateTime.Now.Date.Year - 5, 1, 1);

DateTime endDate = DateTime.Today;



List<Transaction> transactions = [];

decimal balance = startingBalance;
DateTime current = startDate;

while (current <= endDate)
{

    Random frequencyRng = new(Guid.NewGuid().GetHashCode());

    int frequency = frequencyRng.Next(1, 365);

    foreach (var transaction in TransactionTypes)
    {
        if (frequency % transaction.Frequency == 0)
        {
            transactions.Add(GenerateTransaction(transaction, current, ref balance));
        }
    }

    // Salary
    if (current.Day == 1)
    {
        const int salary = 5000;
        balance += salary;
        transactions.Add(new Transaction { Date = current, Credit = salary, Description = "Salary", Balance = balance });
    }

    if (current.Day == 20)
    {
        transactions.Add(GenerateTransaction(new() { Debit = 20, Description = "Netflix" }, current, ref balance));
    }

    if (current.Day == 28)
    {
        transactions.Add(GenerateTransaction(new() { Debit = 20, Description = "Spotify" }, current, ref balance));
    }
    if (current.Day == 15)
    {
        transactions.Add(GenerateTransaction(new() { Debit = 40, Description = "Mobile" }, current, ref balance));
        transactions.Add(GenerateTransaction(new() { Debit = 70, Description = "Internet" }, current, ref balance));
    }

    current = current.AddDays(1);

    // Holiday every year
    if (current.Day == 10 && current.Month == 5)
    {
        transactions.Add(GenerateTransaction(new() { Debit = 2000, Description = "Flights" }, current, ref balance));
    }

    if (current.Day == 12 && current.Month == 5)
    {
        transactions.Add(GenerateTransaction(new() { Debit = 1000, Description = "Hotel" }, current, ref balance));
    }
}


using var csv = File.CreateText(@".\Transactions.csv");
csv.WriteLine("Date,Description,Credit,Debit,Balance");
foreach (var t in transactions)
{
    Console.WriteLine($"{t.Date:yyyy-MM-dd}, {t.Description}, {t.Credit}, {t.Debit}, {t.Balance}");
    csv.WriteLine($"{t.Date:dd/MM/yyyy},{t.Description},{(t.Credit == 0 ? String.Empty : t.Credit)}, {(t.Debit == 0 ? String.Empty : t.Debit)},{t.Balance}");
}

static Transaction GenerateTransaction(Transaction transactionType, DateTime date, ref decimal balance)
{
    Random amountRng = new(Guid.NewGuid().GetHashCode());

    Transaction transaction = (Transaction)transactionType.Clone();

    decimal transactionAmountModifier = Convert.ToDecimal(amountRng.Next(-5, 5) + Math.Round(amountRng.NextDouble(), 2));

    if (transaction.Debit > 0)
    {
        transaction.Debit += transactionAmountModifier;
        balance -= transaction.Debit;
    }
    else if (transaction.Credit > 0)
    {
        transaction.Credit += transactionAmountModifier;
        balance += transaction.Credit;
    }

    transaction.Date = date;
    transaction.Balance = balance;

    return transaction;
}
