using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentIdRequirement() : RouteParamAuthorisationRequirement("instrumentId")
{
}


internal class InstrumentOwnerRequirement : InstrumentIdRequirement
{
}

internal class InstrumentViewerRequirement : InstrumentIdRequirement
{
}
