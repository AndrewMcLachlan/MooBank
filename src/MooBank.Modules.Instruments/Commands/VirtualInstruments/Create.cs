using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Modules.Instruments.Models.Virtual;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Mvc;
using Controller = Asm.MooBank.Controller;
// The domain entity and the model share a name; this file deals in the model.
using VirtualInstrument = Asm.MooBank.Models.VirtualInstrument;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;

public record Create(Guid InstrumentId, [FromBody] CreateVirtualInstrument VirtualInstrument) : ICommand<VirtualInstrument>;

internal class CreateHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<Create, VirtualInstrument>
{
    public async ValueTask<VirtualInstrument> Handle(Create command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, cancellationToken);

        var entity = Domain.Entities.Instrument.VirtualInstrument.Create(
            command.VirtualInstrument.Name,
            command.VirtualInstrument.Description,
            command.VirtualInstrument.Controller,
            instrument.Currency);

        instrument.AddVirtualInstrument(entity, command.VirtualInstrument.OpeningBalance);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await entity.ToModel(currencyConverter, cancellationToken);
    }
}
