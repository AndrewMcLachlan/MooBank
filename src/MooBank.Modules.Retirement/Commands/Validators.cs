using Asm.MooBank.Modules.Retirement.Models;
using FluentValidation;

namespace Asm.MooBank.Modules.Retirement.Commands;

public class CreatePlanValidator : AbstractValidator<CreatePlan>
{
    public CreatePlanValidator()
    {
        RuleFor(x => x.Plan).NotNull().SetValidator(new RetirementPlanBaseValidator());
    }
}

public class UpdatePlanValidator : AbstractValidator<UpdatePlan>
{
    public UpdatePlanValidator()
    {
        RuleFor(x => x.Plan).NotNull().SetValidator(new RetirementPlanBaseValidator());
    }
}

public class RetirementPlanBaseValidator : AbstractValidator<RetirementPlanBase>
{
    public RetirementPlanBaseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        // Wide bounds deliberately: the point is to stop nonsense reaching the projection, not to
        // second-guess what someone wants to model.
        RuleFor(x => x.ExpectedReturnRate)
            .InclusiveBetween(-1m, 1m).WithMessage("Expected return must be between -100% and 100%");

        RuleFor(x => x.InflationRate)
            .InclusiveBetween(-1m, 1m).WithMessage("Inflation must be between -100% and 100%");

        RuleFor(x => x.SuperGuaranteeRate)
            .InclusiveBetween(0m, 1m).WithMessage("Superannuation guarantee rate must be between 0% and 100%");

        RuleFor(x => x.ContributionsTaxRate)
            .InclusiveBetween(0m, 1m).WithMessage("Contributions tax rate must be between 0% and 100%");

        RuleFor(x => x.LifeExpectancy)
            .InclusiveBetween(1, 120).WithMessage("Life expectancy must be between 1 and 120");

        RuleForEach(x => x.Members).SetValidator(new RetirementPlanMemberValidator());
    }
}

public class RetirementPlanMemberValidator : AbstractValidator<RetirementPlanMember>
{
    public RetirementPlanMemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Member name is required")
            .MaximumLength(200).WithMessage("Member name must not exceed 200 characters");

        RuleFor(x => x.CurrentIncome)
            .GreaterThanOrEqualTo(0m).WithMessage("Income cannot be negative");

        RuleFor(x => x.SalarySacrifice)
            .GreaterThanOrEqualTo(0m).WithMessage("Salary sacrifice cannot be negative");

        RuleFor(x => x.CurrentAge)
            .InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120");

        RuleFor(x => x.RetirementAge)
            .InclusiveBetween(1, 120).WithMessage("Retirement age must be between 1 and 120");

        RuleFor(x => x.GrowthStrategy)
            .IsInEnum().WithMessage("Unknown growth strategy");
    }
}
