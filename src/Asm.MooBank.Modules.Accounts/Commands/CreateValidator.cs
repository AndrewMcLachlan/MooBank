using FluentValidation;

namespace Asm.MooBank.Modules.Accounts.Commands;

public class CreateValidator : AbstractValidator<Create>
{
    public CreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        RuleFor(x => x.InstitutionId)
            .GreaterThan(0).WithMessage("Institution is required");
    }
}
