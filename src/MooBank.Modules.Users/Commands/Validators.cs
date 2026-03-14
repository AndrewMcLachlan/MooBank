using Asm.MooBank.Modules.Users.Models;
using FluentValidation;

namespace Asm.MooBank.Modules.Users.Commands;

internal class UpdateValidator : AbstractValidator<Update>
{
    public UpdateValidator()
    {
        RuleFor(x => x.User).NotNull().SetValidator(new UpdateUserValidator());
    }
}

internal class UpdateUserValidator : AbstractValidator<UpdateUser>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        RuleForEach(x => x.Cards).SetValidator(new UserCardValidator());
    }
}

internal class UserCardValidator : AbstractValidator<UserCard>
{
    public UserCardValidator()
    {
        RuleFor(x => x.Last4Digits)
            .InclusiveBetween((short)0, (short)9999).WithMessage("Last 4 digits must be between 0 and 9999");

        RuleFor(x => x.Name)
            .MaximumLength(50).WithMessage("Card name must not exceed 50 characters")
            .When(x => x.Name != null);
    }
}
