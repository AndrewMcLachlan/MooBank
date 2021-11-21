using Asm.BankPlus.Infrastructure;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Transactions
{
    public static class RecurringTransactions
    {
        public static void Process()
        {
            using (BankPlusContext db = new BankPlusContext())
            {
                foreach(var trans in db.RecurringTransactions)
                {
                    if (trans.LastRun == null)
                    {
                        RunTransaction(trans);
                        trans.LastRun = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        DateTime lastRun = trans.LastRun.Value;
                        DateTime now = DateTime.Now;

                        TimeSpan diff = now - lastRun;

                        bool process = false;
                        switch (trans.Schedule)
                        {
                            case Schedule.Daily:
                                process = diff.TotalDays >= 1;
                                break;
                            case Schedule.Weekly:
                                process = diff.TotalDays >= 7;
                                break;
                            case Schedule.Monthly:
                                // Make sure some time has passed and
                                // that we are one calendar month apart or as close as we can be
                                // (e.g. if the transaction last ran on the 31st Jan, the next run will be the 28th Feb).

                                process = diff.TotalDays >= 28 && (lastRun.Day == now.Day || DateTime.DaysInMonth(now.Year, now.Month) < lastRun.Day);
                                break;
                            default:
                                throw new InvalidOperationException("Unsupported schedule: " + trans.Schedule.ToString());
                        }

                        if (process)
                        {
                            RunTransaction(trans);
                            trans.LastRun = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private static void RunTransaction(Data.Entities.RecurringTransaction trans)
        {
            _ = TransactionProcessor.Transfer(trans.Amount, trans.SourceVirtualAccountId, trans.DestinationVirtualAccountId, true, trans.Description);
        }
    }
}
