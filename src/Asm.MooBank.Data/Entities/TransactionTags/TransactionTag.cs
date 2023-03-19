namespace Asm.MooBank.Domain.Entities;

public partial class TransactionTag
{
    public TransactionTag()
    {
        TaggedTo = new HashSet<TransactionTag>();
        Tags = new HashSet<TransactionTag>();
        //Rules = new HashSet<TransactionTagRule>();
    }


    public int TransactionTagId { get; set; }

    public string Name { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }

    public virtual ICollection<TransactionTag> TaggedTo { get; set; }

    public virtual ICollection<TransactionTag> Tags { get; set; }

    //public virtual ICollection<TransactionTagRule> Rules { get; set; }
}
