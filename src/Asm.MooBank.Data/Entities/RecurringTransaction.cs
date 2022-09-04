using System;
using System.Collections.Generic;
using Asm.MooBank.Models;

namespace Asm.MooBank.Data.Entities
{
    public partial class RecurringTransaction
    {
        public int RecurringTransactionId { get; set; }
        public Guid SourceVirtualAccountId { get; set; }
        public Guid DestinationVirtualAccountId { get; set; }
        public string? Description { get; set; }
        public int ScheduleId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? LastRun { get; set; }

        public virtual VirtualAccount DestinationVirtualAccount { get; set; }
        public virtual Schedule Schedule { get; set; }
        public virtual VirtualAccount SourceVirtualAccount { get; set; }
    }
}
