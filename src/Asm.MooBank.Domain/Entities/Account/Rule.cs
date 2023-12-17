﻿using System.Diagnostics.CodeAnalysis;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class Rule : KeyedEntity<int>
{
    public Rule() : base(default)
    {
    }

    public Rule([DisallowNull] int id) : base(id)
    {
    }

    public string? Description { get; set; }

    public Guid AccountId { get; set; }

    public required string Contains { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Tag.Tag> Tags { get; set; } = new HashSet<Tag.Tag>();
}
