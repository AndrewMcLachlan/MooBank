using Asm.MooBank.Tools.TransactionGenerator;

// Parse command line arguments
decimal transactionBalance = 5000m;
decimal savingsBalance = 10000m;
DateTime startDate = new(DateTime.Now.Year - 10, 1, 1);
DateTime endDate = DateTime.Today;
string outputPrefix = "Transactions";
bool generateSavings = true;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--start" or "-s" when i + 1 < args.Length:
            startDate = DateTime.Parse(args[++i]);
            break;
        case "--end" or "-e" when i + 1 < args.Length:
            endDate = DateTime.Parse(args[++i]);
            break;
        case "--balance" or "-b" when i + 1 < args.Length:
            transactionBalance = Decimal.Parse(args[++i]);
            break;
        case "--savings-balance" when i + 1 < args.Length:
            savingsBalance = Decimal.Parse(args[++i]);
            break;
        case "--output" or "-o" when i + 1 < args.Length:
            outputPrefix = args[++i];
            break;
        case "--no-savings":
            generateSavings = false;
            break;
        case "--help" or "-h":
            PrintHelp();
            return;
    }
}

Console.WriteLine("Transaction Generator for MooBank");
Console.WriteLine("==================================");
Console.WriteLine($"Start Date:         {startDate:dd/MM/yyyy}");
Console.WriteLine($"End Date:           {endDate:dd/MM/yyyy}");
Console.WriteLine($"Transaction Balance: ${transactionBalance:N2}");
if (generateSavings)
{
    Console.WriteLine($"Savings Balance:     ${savingsBalance:N2}");
}
Console.WriteLine($"Output Prefix:       {outputPrefix}");
Console.WriteLine();

// Generate transaction account
Console.WriteLine("Generating transaction account...");
var transactionGenerator = new TransactionAccountGenerator(transactionBalance, startDate, endDate);
var transactions = transactionGenerator.Generate();

Console.WriteLine($"Generated {transactions.Count} transactions");
PrintStatistics(transactions, transactionBalance);

// Write transaction account CSV
var transactionFile = generateSavings ? $"{outputPrefix}_Transaction.csv" : $"{outputPrefix}.csv";
WriteCsv(transactionFile, transactions);

// Generate savings account if requested
if (generateSavings)
{
    Console.WriteLine();
    Console.WriteLine("Generating savings account...");

    // Get the transfers from transaction account to savings
    var transfersToSavings = transactions
        .Where(t => t.Category == "Transfer" && t.Debit.HasValue)
        .Select(t => (t.Date, Amount: t.Debit!.Value))
        .ToList();

    var savingsGenerator = new SavingsAccountGenerator(savingsBalance, startDate, endDate, transfersToSavings);
    var savingsTransactions = savingsGenerator.Generate();

    Console.WriteLine($"Generated {savingsTransactions.Count} transactions");
    PrintStatistics(savingsTransactions, savingsBalance);

    // Write savings account CSV
    var savingsFile = $"{outputPrefix}_Savings.csv";
    WriteCsv(savingsFile, savingsTransactions);
}

Console.WriteLine();
Console.WriteLine("Done!");

static void PrintStatistics(List<Transaction> transactions, decimal startingBalance)
{
    var credits = transactions.Where(t => t.Credit.HasValue).Sum(t => t.Credit!.Value);
    var debits = transactions.Where(t => t.Debit.HasValue).Sum(t => t.Debit!.Value);
    var finalBalance = transactions.LastOrDefault()?.Balance ?? startingBalance;

    Console.WriteLine($"  Total Credits:  ${credits:N2}");
    Console.WriteLine($"  Total Debits:   ${debits:N2}");
    Console.WriteLine($"  Net Flow:       ${credits - debits:N2}");
    Console.WriteLine($"  Final Balance:  ${finalBalance:N2}");
}

static void WriteCsv(string filename, List<Transaction> transactions)
{
    Console.WriteLine($"Writing to {filename}...");
    using var csv = File.CreateText(filename);
    csv.WriteLine("Date,Description,Credit,Debit,Balance");

    foreach (var t in transactions)
    {
        csv.WriteLine(t.ToCsv());
    }
}

static void PrintHelp()
{
    Console.WriteLine("Transaction Generator for MooBank");
    Console.WriteLine();
    Console.WriteLine("Generates realistic Australian bank transaction data for demo purposes.");
    Console.WriteLine("Creates ING-compatible CSV files for transaction and savings accounts.");
    Console.WriteLine();
    Console.WriteLine("Usage: TransactionGenerator [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -s, --start <date>       Start date (default: 10 years ago)");
    Console.WriteLine("  -e, --end <date>         End date (default: today)");
    Console.WriteLine("  -b, --balance <amt>      Transaction account starting balance (default: 5000)");
    Console.WriteLine("      --savings-balance    Savings account starting balance (default: 10000)");
    Console.WriteLine("  -o, --output <prefix>    Output file prefix (default: Transactions)");
    Console.WriteLine("      --no-savings         Don't generate savings account");
    Console.WriteLine("  -h, --help               Show this help");
    Console.WriteLine();
    Console.WriteLine("Output Files:");
    Console.WriteLine("  {prefix}_Transaction.csv  - Everyday transaction account");
    Console.WriteLine("  {prefix}_Savings.csv      - Savings account with interest");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  TransactionGenerator -s 2020-01-01 -e 2024-12-31");
    Console.WriteLine("  TransactionGenerator -b 10000 --savings-balance 25000");
    Console.WriteLine("  TransactionGenerator --no-savings -o MyTransactions");
}
