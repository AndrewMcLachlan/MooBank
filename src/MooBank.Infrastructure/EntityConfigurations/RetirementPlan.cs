using Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class RetirementPlanConfiguration : IEntityTypeConfiguration<RetirementPlan>
{
    public void Configure(EntityTypeBuilder<RetirementPlan> entity)
    {
        // A plan is meaningless without its members, and every read of one needs them.
        entity.Navigation(x => x.Members).AutoInclude();
    }
}

internal class RetirementPlanMemberConfiguration : IEntityTypeConfiguration<RetirementPlanMember>
{
    public void Configure(EntityTypeBuilder<RetirementPlanMember> entity)
    {
        entity.Navigation(x => x.Accounts).AutoInclude();
    }
}
