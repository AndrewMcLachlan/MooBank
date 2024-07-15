using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Models;

namespace Asm.MooBank.Modules.Institutions.Queries;

public record GetAll(AccountType? AccountType) : IQuery<IEnumerable<Models.Institution>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Institution.Institution> institutions) : IQueryHandler<GetAll, IEnumerable<Models.Institution>>
{

    public async ValueTask<IEnumerable<Models.Institution>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        if (query.AccountType != null)
        {
            InstitutionType[] institutionTypes = query.AccountType switch
            {
                AccountType.Superannuation => [InstitutionType.SuperannuationFund],
                AccountType.Investment => [InstitutionType.Broker],
                _ => [InstitutionType.Bank, InstitutionType.BuildingSociety, InstitutionType.CreditUnion, InstitutionType.Other],
            };

            institutions = institutions.Where(x => institutionTypes.Contains(x.InstitutionType));
        }

        return await institutions.OrderBy(x => x.Name).ToModel().ToListAsync(cancellationToken);
    }
}
