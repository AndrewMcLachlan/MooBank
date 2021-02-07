using System;
using System.Collections.Generic;
using System.IO;

namespace Asm.BankPlus.Tools.TransactionGenerator
{
    class Program
    {
        private static readonly List<Transaction> TransactionTypes = new List<Transaction>
        {
            new Transaction { Debit = 100, Description = "Supermarket", Frequency = 7 },
            new Transaction { Debit = 20, Description = "Supermarket", Frequency = 3 },
            new Transaction { Debit = 50, Description = "Bottle Shop", Frequency = 14 },
            new Transaction { Debit = 5, Description = "Coffee Shop", Frequency = 2 },
            new Transaction { Debit = 70, Description = "Restaurant", Frequency = 14 },
            new Transaction { Debit = 100, Description = "Clothes Shop", Frequency = 14 },
            new Transaction { Debit = 30, Description = "Takeaway", Frequency = 14 },
            new Transaction { Debit = 300, Description = "Electricity Bill", Frequency = 90 },
            new Transaction { Debit = 400, Description = "Tax", Frequency = 90 },
            new Transaction { Debit = 500, Description = "Water Nill", Frequency = 90 },
            new Transaction { Debit = 100, Description = "Pharmacy", Frequency = 30 },
            new Transaction { Debit = 60, Description = "Fuel", Frequency = 14 },
        };


        static void Main(string[] args)
        {
            decimal startingBalance = 5000;

            DateTime startDate = new DateTime(DateTime.Now.Date.Year - 1, 1, 1);

            DateTime endDate = new DateTime(DateTime.Now.Date.Year - 1, 12, 31);



            List<Transaction> transactions = new List<Transaction>();

            decimal balance = startingBalance;
            DateTime current = startDate;
            Random transactionsPerDayRng = new Random(Guid.NewGuid().GetHashCode());

            while (current <= endDate)
            {
                var transactionsPerDay = transactionsPerDayRng.Next(1, 4);

                //transactions.Add(GenerateTransaction(ref balance));
                Random freqRng = new Random(Guid.NewGuid().GetHashCode());

                int freq = freqRng.Next(1, 365);

                foreach (var tran in TransactionTypes)
                {
                    if (freq % tran.Frequency == 0)
                    {
                        transactions.Add(GenerateTransaction(tran, current, ref balance));
                    }
                }

                if (current.Day == 15)
                {
                    const int salary = 2500;
                    balance += salary;
                    transactions.Add(new Transaction { Date = current, Credit = salary, Description = "Salary", Balance = balance });
                }

                current = current.AddDays(1);
            }

            using var csv = File.CreateText(@".\Transactions.csv");
            csv.WriteLine("Date,Description,Credit,Debit,Balance");
            foreach (var tran in transactions)
            {
                Console.WriteLine($"{tran.Date:yyyy-MM-dd}, {tran.Description}, {tran.Credit}, {tran.Debit}, {tran.Balance}");
                csv.WriteLine($"{tran.Date:dd/MM/yyyy},{tran.Description},{(tran.Credit == 0 ? String.Empty : tran.Credit)}, {(tran.Debit == 0 ? String.Empty : tran.Debit)},{tran.Balance}");
            }
        }

        private static Transaction GenerateTransaction(Transaction transactionType, DateTime date, ref decimal balance)
        {
            Random amountRng = new Random(Guid.NewGuid().GetHashCode());

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

    }
}