using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentIdRequirement : RouteParamAuthorisationRequirement
{
    public InstrumentIdRequirement() : base("instrumentId")
    {
    }

    public InstrumentIdRequirement(string id) : base(id)
    {
    }
}


internal class InstrumentOwnerRequirement : InstrumentIdRequirement
{
    public InstrumentOwnerRequirement() : base()
    {
    }

    public InstrumentOwnerRequirement(string id) : base(id)
    {
    }
}

internal class InstrumentViewerRequirement : InstrumentIdRequirement
{
    public InstrumentViewerRequirement() : base()
    {
    }

    public InstrumentViewerRequirement(string id) : base(id)
    {
    }
}
