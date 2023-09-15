using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Transactions;
public class TransactionSplit : KeyedEntity<Guid>
{
    public TransactionSplit() : base(Guid.NewGuid())
    {
    }

    public TransactionSplit([DisallowNull] Guid id) : base(id)
    {
    }

    public Guid TransactionId { get; set; }

    public decimal Amount { get; set; }

    public virtual ICollection<Tag.Tag> Tags { get; set; } = new HashSet<Tag.Tag>();
}
