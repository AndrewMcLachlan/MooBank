
using Asm.MooBank.Infrastructure;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Hello, World!");


DbContextOptionsBuilder<BankPlusContext> builder = new();

builder.UseSqlServer("Server=.\\SQLExpress;Database=BankPlus;Trusted_Connection=True;Trust Server Certificate=true");

BankPlusContext context = new BankPlusContext(builder.Options);

var account = context.Accounts.Where(a => a.AccountId == new Guid("a1bba1b7-0f3b-4c75-a6d6-822d98721bc5")).ToList();