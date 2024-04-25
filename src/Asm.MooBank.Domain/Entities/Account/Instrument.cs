﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public abstract class Instrument(Guid id) : KeyedEntity<Guid>(id)
{
    public Instrument() : this(Guid.Empty)
    {
    }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string Currency { get; set; } = "AUD";

    public decimal Balance { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public bool ShareWithFamily { get; set; }

    [NotMapped]
    public string? Slug { get; set; }

    public virtual ICollection<InstrumentOwner> Owners { get; set; } = [];

    public virtual ICollection<InstrumentViewer> Viewers { get; set; } = [];


    public virtual ICollection<Rule> Rules { get; set; } = new HashSet<Rule>();

    public virtual ICollection<VirtualInstrument> VirtualInstruments { get; set; } = [];

    public virtual Group.Group? GetAccountGroup(Guid accountHolderId) =>
        Owners.Where(a => a.UserId == accountHolderId).Select(aah => aah.Group).SingleOrDefault();

    public void SetAccountGroup(Guid? groupId, Guid currentUserId)
    {
        var existing = Owners.SingleOrDefault(aah => aah.UserId == currentUserId);

        if (existing == null)
        {
            var existingViewer = Viewers.SingleOrDefault(av => av.UserId == currentUserId);

            if (existingViewer != null)
            {
                existingViewer.GroupId = groupId;
            }
            else
            {
                Viewers.Add(new InstrumentViewer
                {
                    GroupId = groupId,
                    UserId = currentUserId,
                });
            }
        }
        else
        {

            existing.GroupId = groupId;
        }
    }

    public void SetAccountHolder(Guid currentUserId)
    {
        var existing = Owners.SingleOrDefault(aah => aah.UserId == currentUserId);

        if (existing != null) throw new ExistsException("User is already an account holder");

        Owners.Add(new InstrumentOwner
        {
            UserId = currentUserId,
        });
    }
}
