using System.ComponentModel;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Modules.ReferenceData.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.ReferenceData.Commands;

[DisplayName("SaveGrowthStrategyRate")]
public record SaveGrowthStrategyRate([FromBody] GrowthStrategyRates Rate) : ICommand<GrowthStrategyRates>;

/// <remarks>
/// Update only. The strategies are the enum and arrive with the schema, so there is never a new one
/// to add — only a rate to correct.
/// </remarks>
internal class SaveGrowthStrategyRateHandler(
    IReferenceDataRepository referenceDataRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SaveGrowthStrategyRate, GrowthStrategyRates>
{
    public async ValueTask<GrowthStrategyRates> Handle(SaveGrowthStrategyRate command, CancellationToken cancellationToken)
    {
        var entity = await referenceDataRepository.GetGrowthStrategyRate(command.Rate.Strategy, cancellationToken);

        entity.Rate = command.Rate.Rate;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return command.Rate with { Description = entity.Description };
    }
}

/// <summary>
/// The command-level validator, which is what the endpoint's validation filter looks for. Without
/// one the rules below never run.
/// </summary>
public class SaveGrowthStrategyRateCommandValidator : AbstractValidator<SaveGrowthStrategyRate>
{
    public SaveGrowthStrategyRateCommandValidator()
    {
        RuleFor(x => x.Rate).NotNull().SetValidator(new SaveGrowthStrategyRateValidator());
    }
}

public class SaveGrowthStrategyRateValidator : AbstractValidator<GrowthStrategyRates>
{
    public SaveGrowthStrategyRateValidator()
    {
        RuleFor(x => x.Strategy)
            .NotEqual(GrowthStrategy.Custom).WithMessage("A custom rate belongs to the member who chose it, not to reference data");

        RuleFor(x => x.Rate)
            .NotNull().WithMessage("A rate is required")
            .InclusiveBetween(-1m, 1m).WithMessage("The rate must be between -100% and 100%");
    }
}
