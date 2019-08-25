using System.Collections.Generic;

namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionCategory
    {
        public TransactionCategory()
        {
            Children = new HashSet<TransactionCategory>();
            TransactionCategoryRules = new HashSet<TransactionCategoryRule>();
        }

        public int TransactionCategoryId { get; set; }

        public string Description { get; set; }

        public bool IsLivingExpense { get; set; }

        public int? ParentCategoryId { get; set; }

        public bool Deleted { get; set; }

        public virtual TransactionCategory Parent { get; set; }

        public virtual ICollection<TransactionCategory> Children { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual ICollection<TransactionCategoryRule> TransactionCategoryRules { get; set; }
    }
}
