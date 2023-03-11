﻿using System;
using System.Collections.Generic;

namespace Asm.MooBank.Data.Entities
{
    public partial class TransactionTransactionTag
    {
        public TransactionTransactionTag()
        {
        }

        public Guid TransactionId { get; set; }

        public int TransactionTagId { get; set; }

        public virtual Transaction Transaction { get; set; }

        public virtual TransactionTag TransactionTag { get; set; }
    }
}