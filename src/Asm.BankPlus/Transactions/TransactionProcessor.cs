using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
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

                var source = new Data.Models.Transaction
                {
                    Amount = amount,
                    TransactionType = sourceType,
                    TransactionGroupId = groupId,
                    VirtualAccountId = sourceAccountId,
                    Description = description,
                };

                var destination = new Data.Models.Transaction
                {
                    Amount = amount,
                    TransactionType = destinationType,
                    TransactionGroupId = groupId,
                    VirtualAccountId = destinationAccountId,
                    Description = description,
                };

                db.Transaction.Add(source);
                db.Transaction.Add(destination);

                db.SaveChanges();
            }
        }
    }
}
