using Asm.MooBank.Modules.Budgets.Models;
using FluentValidation;

namespace Asm.MooBank.Modules.Budgets.Commands;

public class CreateLineValidator : AbstractValidator<CreateLine>
{
    public CreateLineValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween((short)2000, (short)2100).WithMessage("Year must be between 2000 and 2100");

        RuleFor(x => x.BudgetLine).NotNull().SetValidator(new BudgetLineBaseValidator());
    }
}

public class UpdateLineValidator : AbstractValidator<UpdateLine>
{
    public UpdateLineValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween((short)2000, (short)2100).WithMessage("Year must be between 2000 and 2100");

        RuleFor(x => x.BudgetLine).NotNull().SetValidator(new BudgetLineBaseValidator());
    }
}

public class BudgetLineBaseValidator : AbstractValidator<BudgetLineBase>
{
    public BudgetLineBaseValidator()
    {
        RuleFor(x => x.TagId)
            .GreaterThan(0).WithMessage("Tag is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Month)
            .InclusiveBetween((short)0, (short)12).WithMessage("Month must be between 0 and 12");

        RuleFor(x => x.Notes)
            .MaximumLength(512).WithMessage("Notes must not exceed 512 characters")
            .When(x => x.Notes != null);
    }
}
