namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionCategory
    {
        public static implicit operator Models.TransactionCategory(TransactionCategory transactionCategory)
        {
            return new Models.TransactionCategory()
            {
                Id = transactionCategory.TransactionCategoryId,
                Description = transactionCategory.Description,
                IsLivingExpense = transactionCategory.IsLivingExpense,
                ParentCategoryId = transactionCategory.ParentCategoryId,
            };
        }

        public static implicit operator TransactionCategory(Models.TransactionCategory transactionCategory)
        {
            return new TransactionCategory
            {
                TransactionCategoryId = transactionCategory.Id,
                Description = transactionCategory.Description,
                IsLivingExpense = transactionCategory.IsLivingExpense,
                ParentCategoryId = transactionCategory.ParentCategoryId,
            };
        }

    }
}
