using Asm.MooBank.Models;
using Asm.MooBank.Modules.Tags.Models;
using FluentValidation;

namespace Asm.MooBank.Modules.Tags.Commands;

public class CreateValidator : AbstractValidator<Create>
{
    public CreateValidator()
    {
        RuleFor(x => x.Tag).NotNull().SetValidator(new TagValidator());
    }
}

public class UpdateValidator : AbstractValidator<Update>
{
    public UpdateValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Tag ID is required");
        RuleFor(x => x.Tag).NotNull().SetValidator(new UpdateTagValidator());
    }
}

public class TagValidator : AbstractValidator<Tag>
{
    public TagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}

public class UpdateTagValidator : AbstractValidator<UpdateTag>
{
    public UpdateTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}
