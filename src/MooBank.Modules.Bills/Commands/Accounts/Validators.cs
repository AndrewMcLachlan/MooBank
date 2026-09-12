using FluentValidation;

namespace Asm.MooBank.Modules.Bills.Commands.Accounts;

public class CreateValidator : AbstractValidator<Create>
{
    public CreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description must not exceed 255 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .MaximumLength(20).WithMessage("Account number must not exceed 20 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");
    }
}

public class UpdateValidator : AbstractValidator<Update>
{
    public UpdateValidator()
    {
        RuleFor(x => x.Account.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.Account.Description)
            .MaximumLength(255).WithMessage("Description must not exceed 255 characters")
            .When(x => x.Account.Description != null);

        RuleFor(x => x.Account.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .MaximumLength(15).WithMessage("Account number must not exceed 15 characters");
    }
}
