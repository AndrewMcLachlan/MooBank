using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public abstract class Instrument : KeyedEntity<Guid>
{
    private readonly List<VirtualInstrument> _virtualInstruments = [];

    protected Instrument(Guid id) : base(id)
    {
    }

    // For EF materialisation only. Construct instruments through the concrete aggregate factories.
    private Instrument() : this(Guid.Empty)
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

    public DateOnly? ClosedDate { get; set; }

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

    protected void MarkCreated() => Events.Add(new InstrumentCreatedEvent(this));

    protected void MarkUpdated() => Events.Add(new InstrumentUpdatedEvent(this));

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

    public Rule AddRule(string contains, string? description, IEnumerable<Tag.Tag> tags)
    {
        var rule = new Rule
        {
            InstrumentId = Id,
            Contains = contains,
            Description = description,
            Tags = [.. tags],
        };

        Rules.Add(rule);

        return rule;
    }

    public Rule UpdateRule(int ruleId, string contains, string? description)
    {
        var rule = GetRule(ruleId);

        rule.Contains = contains;
        rule.Description = description;

        return rule;
    }

    public void RemoveRule(int ruleId) => Rules.Remove(GetRule(ruleId));

    private Rule GetRule(int ruleId) =>
        Rules.SingleOrDefault(r => r.Id == ruleId) ?? throw new NotFoundException($"Rule with ID {ruleId} not found");
}
