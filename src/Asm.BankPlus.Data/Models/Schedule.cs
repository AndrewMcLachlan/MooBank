using System;
using System.Collections.Generic;

namespace Asm.BankPlus.Data.Models
{
    public partial class Schedule
    {
        public Schedule()
        {
            RecurringTransaction = new HashSet<RecurringTransaction>();
        }

        public int ScheduleId { get; set; }
        public string Description { get; set; }

        public virtual ICollection<RecurringTransaction> RecurringTransaction { get; set; }
    }
}
