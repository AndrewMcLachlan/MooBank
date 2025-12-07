using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Domain.Entities.Utility;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;

public record UpdateBalance(Guid InstrumentId, Guid VirtualInstrumentId, decimal Balance) : ICommand<VirtualInstrument>
{
    public static async ValueTask<UpdateBalance?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["instrumentId"] as string, out Guid instrumentId)) throw new BadHttpRequestException("invalid instrument ID");
        if (!Guid.TryParse(httpContext.Request.RouteValues["virtualInstrumentId"] as string, out Guid virtualInstrumentId)) throw new BadHttpRequestException("invalid virtual account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<UpdateBalance>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { InstrumentId = instrumentId, VirtualInstrumentId = virtualInstrumentId };
    }
}

internal class UpdateBalanceHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<UpdateBalance, VirtualInstrument>
{
    public async ValueTask<VirtualInstrument> Handle(UpdateBalance command, CancellationToken cancellationToken)
    {
        var parentInstrument = await instrumentRepository.Get(command.InstrumentId, new VirtualAccountSpecification(), cancellationToken);

        var instrument = parentInstrument.VirtualInstruments.SingleOrDefault(a => a.Id == command.VirtualInstrumentId) ?? throw new NotFoundException();

        var amount = command.Balance - instrument.Balance;

        instrument.Events.Add(new BalanceAdjustmentEvent(instrument, amount, "Web"));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return instrument.ToModel(currencyConverter);
    }
}
