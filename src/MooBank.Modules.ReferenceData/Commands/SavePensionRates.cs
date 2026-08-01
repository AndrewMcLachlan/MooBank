using System.ComponentModel;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Modules.ReferenceData.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.ReferenceData.Commands;

[DisplayName("SavePensionRates")]
public record SavePensionRates([FromBody] PensionRates Rates) : ICommand<PensionRates>;

/// <remarks>
/// One command for both new and corrected rates, keyed on the effective date rather than the id: a
/// set of rates is identified by when it came into force, and correcting a typo in this March's
/// figures should not leave two rows claiming the same date.
/// </remarks>
internal class SavePensionRatesHandler(
    IReferenceDataRepository referenceDataRepository,
    IQueryable<PensionRate> rates,
    IUnitOfWork unitOfWork) : ICommandHandler<SavePensionRates, PensionRates>
{
    public async ValueTask<PensionRates> Handle(SavePensionRates command, CancellationToken cancellationToken)
    {
        var existingId = await rates
            .Where(r => r.EffectiveFrom == command.Rates.EffectiveFrom)
            .Select(r => (int?)r.Id)
            .SingleOrDefaultAsync(cancellationToken);

        var entity = existingId is null
            ? referenceDataRepository.AddPensionRate()
            : await referenceDataRepository.GetPensionRate(existingId.Value, cancellationToken);

        entity.EffectiveFrom = command.Rates.EffectiveFrom;
        entity.EligibilityAge = command.Rates.EligibilityAge;
        entity.MaxAnnualSingle = command.Rates.MaxAnnualSingle;
        entity.MaxAnnualCouple = command.Rates.MaxAnnualCouple;
        entity.AssetsFreeAreaSingle = command.Rates.AssetsFreeAreaSingle;
        entity.AssetsFreeAreaCouple = command.Rates.AssetsFreeAreaCouple;
        entity.AssetsTaperRate = command.Rates.AssetsTaperRate;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return command.Rates with { Id = entity.Id };
    }
}

/// <summary>
/// The command-level validator, which is what the endpoint's validation filter looks for. Without
/// one the rules below never run.
/// </summary>
public class SavePensionRatesCommandValidator : AbstractValidator<SavePensionRates>
{
    public SavePensionRatesCommandValidator()
    {
        RuleFor(x => x.Rates).NotNull().SetValidator(new SavePensionRatesValidator());
    }
}

public class SavePensionRatesValidator : AbstractValidator<PensionRates>
{
    public SavePensionRatesValidator()
    {
        RuleFor(x => x.EligibilityAge)
            .InclusiveBetween(50, 80).WithMessage("Eligibility age must be between 50 and 80");

        RuleFor(x => x.MaxAnnualSingle)
            .GreaterThanOrEqualTo(0m).WithMessage("The single rate cannot be negative");

        RuleFor(x => x.MaxAnnualCouple)
            .GreaterThanOrEqualTo(0m).WithMessage("The couple rate cannot be negative");

        RuleFor(x => x.AssetsFreeAreaSingle)
            .GreaterThanOrEqualTo(0m).WithMessage("The single free area cannot be negative");

        RuleFor(x => x.AssetsFreeAreaCouple)
            .GreaterThanOrEqualTo(0m).WithMessage("The couple free area cannot be negative");

        RuleFor(x => x.AssetsTaperRate)
            .InclusiveBetween(0m, 1m).WithMessage("The taper rate must be between 0 and 100%");
    }
}
