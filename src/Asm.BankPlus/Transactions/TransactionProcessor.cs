using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.DataAccess;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Transactions
{
    public static class TransactionProcessor
    {
        public static void Process(decimal amount, Guid sourceAccountId, Guid destinationAccountId, bool isRecurring, string description = null)
        {
            TransactionType sourceType = isRecurring ? TransactionType.RecurringDebit : TransactionType.Debit;
            TransactionType destinationType = isRecurring ? TransactionType.RecurringCredit : TransactionType.Credit;

            using (BankPlusContext db = new BankPlusContext())
            {
                Guid groupId = Guid.NewGuid();

                Transaction source = new Transaction
                {
                    Amount = amount,
                    TransactionType = sourceType,
                    TransactionGroupId = groupId,
                    VirtualAccountId = sourceAccountId,
                    Description = description,
                };

                Transaction destination = new Transaction
                {
                    Amount = amount,
                    TransactionType = destinationType,
                    TransactionGroupId = groupId,
                    VirtualAccountId = destinationAccountId,
                    Description = description,
                };

                db.Transactions.Add(source);
                db.Transactions.Add(destination);

                db.SaveChanges();
            }
        }
    }
}
