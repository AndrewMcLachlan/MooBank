﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public abstract class Account(Guid id) : KeyedEntity<Guid>(id)
{
    public Account() : this(Guid.Empty)
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

    public virtual ICollection<AccountAccountHolder> AccountHolders { get; set; } = new HashSet<AccountAccountHolder>();

    public virtual ICollection<AccountAccountViewer> AccountViewers { get; set; } = new HashSet<AccountAccountViewer>();


    public virtual ICollection<Rule> Rules { get; set; } = new HashSet<Rule>();

    public virtual ICollection<VirtualAccount> VirtualAccounts { get; set; } = new HashSet<VirtualAccount>();

    public virtual AccountGroup.AccountGroup? GetAccountGroup(Guid accountHolderId) =>
        AccountHolders.Where(a => a.AccountHolderId == accountHolderId).Select(aah => aah.AccountGroup).SingleOrDefault();

    public void SetAccountGroup(Guid? accountGroupId, Guid currentUserId)
    {
        var existing = AccountHolders.SingleOrDefault(aah => aah.AccountHolderId == currentUserId);

        if (existing == null)
        {
            var existingViewer = AccountViewers.SingleOrDefault(av => av.AccountHolderId == currentUserId);

            if (existingViewer != null)
            {
                existingViewer.AccountGroupId = accountGroupId;
            }
            else
            {
                AccountViewers.Add(new AccountAccountViewer
                {
                    AccountGroupId = accountGroupId,
                    AccountHolderId = currentUserId,
                });
            }
        }
        else
        {

            existing.AccountGroupId = accountGroupId;
        }
    }

    public void SetAccountHolder(Guid currentUserId)
    {
        var existing = AccountHolders.SingleOrDefault(aah => aah.AccountHolderId == currentUserId);

        if (existing != null) throw new ExistsException("User is already an account holder");

        AccountHolders.Add(new AccountAccountHolder
        {
            AccountHolderId = currentUserId,
        });
    }
}
