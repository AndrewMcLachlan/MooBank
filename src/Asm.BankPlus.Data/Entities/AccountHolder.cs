namespace Asm.BankPlus.Data.Entities
{
    public partial class AccountHolder
    {
        public Guid AccountHolderId { get; set; }
        public string EmailAddress { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
