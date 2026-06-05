using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

public class LogicalAccountRepository(MooBankContext dataContext, User user) : RepositoryDeleteBase<LogicalAccount, Guid>(dataContext), ILogicalAccountRepository
{
    public LogicalAccount Add(LogicalAccount entity, decimal openingBalance, DateOnly openedDate)
    {
        var tracked = base.Add(entity);
        tracked.Events.Add(new InstrumentCreatedEvent(tracked));
        tracked.Events.Add(new AccountAddedEvent(tracked, openingBalance, openedDate));
        return tracked;
    }

    public override LogicalAccount Update(LogicalAccount entity)
    {
        var tracked = base.Update(entity);
        tracked.Events.Add(new InstrumentUpdatedEvent(tracked));
        return tracked;
    }

    protected override IQueryable<LogicalAccount> GetById(Guid id) => Entities.Include(a => a.Owners).Include(t => t.InstitutionAccounts).ThenInclude(i => i!.Institution).Where(a => a.Id == id && a.Owners.Any(ah => ah.UserId == user.Id || (a.ShareWithFamily && ah.User.FamilyId == user.FamilyId)));

    public void RemoveImportAccount(ImportAccount importAccount)
    {
        Context.Remove(importAccount);
    }
}
