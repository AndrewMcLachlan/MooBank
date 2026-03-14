using Asm.MooBank.Modules.Forecast.Models;
using FluentValidation;

namespace Asm.MooBank.Modules.Forecast.Commands;

public class CreatePlanValidator : AbstractValidator<CreatePlan>
{
    public CreatePlanValidator()
    {
        RuleFor(x => x.Plan).NotNull().SetValidator(new ForecastPlanBaseValidator());
    }
}

public class UpdatePlanValidator : AbstractValidator<UpdatePlan>
{
    public UpdatePlanValidator()
    {
        RuleFor(x => x.Plan).NotNull().SetValidator(new ForecastPlanBaseValidator());
    }
}

public class CreatePlannedItemValidator : AbstractValidator<CreatePlannedItem>
{
    public CreatePlannedItemValidator()
    {
        RuleFor(x => x.Item).NotNull().SetValidator(new PlannedItemBaseValidator());
    }
}

public class UpdatePlannedItemValidator : AbstractValidator<UpdatePlannedItem>
{
    public UpdatePlannedItemValidator()
    {
        RuleFor(x => x.Item).NotNull().SetValidator(new PlannedItemBaseValidator());
    }
}

public class ForecastPlanBaseValidator : AbstractValidator<ForecastPlanBase>
{
    public ForecastPlanBaseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.CurrencyCode)
            .Length(3).WithMessage("Currency code must be a 3-letter ISO code")
            .When(x => x.CurrencyCode != null);
    }
}

public class PlannedItemBaseValidator : AbstractValidator<PlannedItemBase>
{
    public PlannedItemBaseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Notes)
            .MaximumLength(512).WithMessage("Notes must not exceed 512 characters")
            .When(x => x.Notes != null);
    }
}
