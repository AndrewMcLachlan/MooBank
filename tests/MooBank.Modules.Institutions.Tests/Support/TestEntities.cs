using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Bogus;

namespace Asm.MooBank.Modules.Institutions.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static Institution CreateInstitution(
        int id = 1,
        string? name = null,
        InstitutionType institutionType = InstitutionType.Bank)
    {
        return new Institution(id)
        {
            Name = name ?? Faker.Company.CompanyName(),
            InstitutionType = institutionType,
        };
    }

    public static List<Institution> CreateSampleInstitutions()
    {
        return
        [
            CreateInstitution(1, "Alpha Bank", InstitutionType.Bank),
            CreateInstitution(2, "Beta Credit Union", InstitutionType.CreditUnion),
            CreateInstitution(3, "Gamma Building Society", InstitutionType.BuildingSociety),
            CreateInstitution(4, "Delta Super Fund", InstitutionType.SuperannuationFund),
            CreateInstitution(5, "Epsilon Broker", InstitutionType.Broker),
            CreateInstitution(6, "Zeta Other", InstitutionType.Other),
        ];
    }

    public static IQueryable<Institution> CreateInstitutionQueryable(IEnumerable<Institution> institutions)
    {
        return QueryableHelper.CreateAsyncQueryable(institutions);
    }

    public static IQueryable<Institution> CreateInstitutionQueryable(params Institution[] institutions)
    {
        return CreateInstitutionQueryable(institutions.AsEnumerable());
    }
}
