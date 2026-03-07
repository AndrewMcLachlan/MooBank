using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;

[DisplayName("UpdateVirtualInstrument")]
public record Update(Guid InstrumentId, Guid VirtualInstrumentId, string Name, string Description, decimal CurrentBalance) : ICommand<VirtualInstrument>
{
    public static async ValueTask<Update?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["instrumentId"] as string, out Guid instrumentId)) throw new BadHttpRequestException("invalid account ID");
        if (!Guid.TryParse(httpContext.Request.RouteValues["virtualInstrumentId"] as string, out Guid virtualInstrumentId)) throw new BadHttpRequestException("invalid account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { InstrumentId = instrumentId, VirtualInstrumentId = virtualInstrumentId };
    }
}

internal class UpdateHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<Update, VirtualInstrument>
{
    public async ValueTask<VirtualInstrument> Handle(Update command, CancellationToken cancellationToken)
    {
        var parentInstrument = await instrumentRepository.Get(command.InstrumentId, new VirtualAccountSpecification(), cancellationToken);

        var instrument = parentInstrument.VirtualInstruments.SingleOrDefault(a => a.Id == command.VirtualInstrumentId) ?? throw new NotFoundException();

        instrument.Name = command.Name;
        instrument.Description = command.Description;

        var amount = instrument.Balance - command.CurrentBalance;

        if (amount > 0)
        {
            instrument.Balance = command.CurrentBalance;

            instrument.Events.Add(new BalanceAdjustmentEvent(instrument, amount, "Web"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return instrument.ToModel(currencyConverter);
    }
}
