﻿using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Entities.StockHolding;

[AggregateRoot]
public class StockHolding(Guid id) : Account.Account(id)
{
    public string Symbol { get; set; } = null!;

    public string Exchange { get; set; } = null!;

    [NotMapped]
    public string InternationalSymbol
    {
        get => $"{Symbol}.{Exchange}";
    }

    public int Quantity { get; set; }

    public decimal CurrentPrice { get; set; }

    public decimal CurrentValue { get; set; }

    public new DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.Now;

    public ICollection<StockTransaction> Transactions { get; set; } = [];

    [NotMapped]
    public IEnumerable<AccountAccountViewer> ValidAccountViewers
    {
        get
        {
            if (!ShareWithFamily) return [];
            var familyIds = base.AccountHolders.Select(a => a.AccountHolder.FamilyId).Distinct();
            return AccountViewers.Where(a => familyIds.Contains(a.AccountHolder.FamilyId));
        }
    }

    public override AccountGroup.AccountGroup? GetAccountGroup(Guid accountHolderId) =>
        base.GetAccountGroup(accountHolderId) ??
        ValidAccountViewers.Where(a => a.AccountHolderId == accountHolderId).Select(aah => aah.AccountGroup).SingleOrDefault();
}