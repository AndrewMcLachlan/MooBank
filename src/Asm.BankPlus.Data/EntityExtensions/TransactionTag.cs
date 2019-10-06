namespace Asm.BankPlus.Data.Entities
{
    public partial class TransactionTag
    {
        public static implicit operator Models.TransactionTag(TransactionTag transactionTag)
        {
            return new Models.TransactionTag()
            {
                Id = transactionTag.TransactionTagId,
                Name = transactionTag.Name,
            };
        }

        public static implicit operator TransactionTag(Models.TransactionTag transactionTag)
        {
            return new TransactionTag
            {
                TransactionTagId = transactionTag.Id,
                Name = transactionTag.Name,
            };
        }

    }
}
