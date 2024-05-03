﻿using System.ComponentModel.DataAnnotations.Schema;
using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Domain.Entities.User;

[AggregateRoot]
public partial class User(Guid id) : KeyedEntity<Guid>(id)
{
    public User() : this(default) { }

    public required string EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string Currency { get; set; } = "AUD";
    public required Guid FamilyId { get; set; }

    public Guid? PrimaryAccountId { get; set; }

    [NotMapped]
    public IEnumerable<Instrument.Instrument> Instruments => InstrumentOwners?.Select(a => a.Instrument) ?? [];

    public virtual ICollection<InstrumentOwner> InstrumentOwners { get; set; } = new HashSet<InstrumentOwner>();

    public virtual ICollection<UserCard> Cards { get; set; } = new HashSet<UserCard>();

    public Family.Family Family { get; set; } = null!;
}