using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public abstract class Instrument(Guid id) : KeyedEntity<Guid>(id)
{
    private readonly List<VirtualInstrument> _virtualInstruments = [];

    public Instrument() : this(Guid.Empty)
    {
    }


    [Required]
    [StringLength(50)]
    public required string Name { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public string Currency { get; set; } = "AUD";

    [Column(TypeName = "datetimeoffset(0)")]
    public DateTimeOffset LastUpdated { get; set; }

    public bool ShareWithFamily { get; set; }

    [Column("ControllerId")]
    public Controller Controller { get; set; }

    [NotMapped]
    public string? Slug { get; set; }

    public virtual ICollection<InstrumentOwner> Owners { get; set; } = [];

    public virtual ICollection<InstrumentViewer> Viewers { get; set; } = [];

    [NotMapped]
    public virtual IEnumerable<Guid> PermittedUsers => Owners.Select(aah => aah.UserId).Union(Viewers.Select(av => av.UserId));

    public virtual ICollection<Rule> Rules { get; set; } = [];

    public IReadOnlyCollection<VirtualInstrument> VirtualInstruments { get => _virtualInstruments; internal init => _virtualInstruments = [.. value]; }

    public virtual Group.Group? GetGroup(Guid accountHolderId) =>
        Owners.Where(a => a.UserId == accountHolderId).Select(aah => aah.Group).SingleOrDefault();

    public void SetGroup(Guid? groupId, Guid currentUserId)
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

    public void AddVirtualInstrument(VirtualInstrument virtualInstrument, decimal openingBalance)
    {
        _virtualInstruments.Add(virtualInstrument);
        Events.Add(new VirtualInstrumentAddedEvent(virtualInstrument, openingBalance));
    }

    public void RemoveVirtualInstrument(Guid virtualInstrumentId)
    {
        var virtualInstrument = _virtualInstruments.SingleOrDefault(a => a.Id == virtualInstrumentId) ?? throw new NotFoundException("Virtual instrument not found");
        _virtualInstruments.Remove(virtualInstrument);
    }
}
