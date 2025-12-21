using FluentValidation;

namespace Asm.MooBank.Modules.Groups.Commands;

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
    }
}

public class UpdateValidator : AbstractValidator<Update>
{
    public UpdateValidator()
    {
        RuleFor(x => x.Group).NotNull().SetValidator(new GroupValidator());
    }
}

public class GroupValidator : AbstractValidator<Models.Group>
{
    public GroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => x.Description != null);
    }
}
