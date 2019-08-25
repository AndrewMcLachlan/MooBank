using System;
using System.Collections.Generic;
using System.Text;

namespace Asm.BankPlus.Models
{
    public class TransactionCategory
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public bool IsLivingExpense { get; set; }

        public int? ParentCategoryId { get; set; }
    }
}
