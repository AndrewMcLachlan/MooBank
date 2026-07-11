using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

public class LogicalAccountRepository(MooBankContext dataContext, User user) : RepositoryDeleteBase<MooBankContext, LogicalAccount, Guid>(dataContext), ILogicalAccountRepository
{
    public override void Delete(Guid id)
    {
        var account = GetById(id).SingleOrDefault() ?? throw new NotFoundException();
        account.ClosedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    protected override IQueryable<LogicalAccount> GetById(Guid id) => Entities.Include(a => a.Owners).Include(t => t.InstitutionAccounts).ThenInclude(i => i!.Institution).Where(a => a.Id == id && a.Owners.Any(ah => ah.UserId == user.Id || (a.ShareWithFamily && ah.User.FamilyId == user.FamilyId)));

}
