using FluentValidation;

namespace Asm.MooBank.Modules.Assets.Commands;

public class CreateValidator : AbstractValidator<Create>
{
    public CreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(255).WithMessage("Description must not exceed 255 characters");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price must not be negative")
            .When(x => x.PurchasePrice.HasValue);

        RuleFor(x => x.CurrentBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Current balance must not be negative");
    }
}

public class UpdateValidator : AbstractValidator<Update>
{
    public UpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(255).WithMessage("Description must not exceed 255 characters");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price must not be negative")
            .When(x => x.PurchasePrice.HasValue);

        RuleFor(x => x.CurrentBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Current balance must not be negative");
    }
}
