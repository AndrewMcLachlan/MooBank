using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;

public class InstrumentIdRequirement : RouteParamAuthorisationRequirement
{
    public InstrumentIdRequirement() : base("instrumentId")
    {
    }

    public InstrumentIdRequirement(string id) : base(id)
    {
    }
}


public class InstrumentOwnerRequirement : InstrumentIdRequirement
{
    public InstrumentOwnerRequirement() : base()
    {
    }

    public InstrumentOwnerRequirement(string id) : base(id)
    {
    }
}

public class InstrumentViewerRequirement : InstrumentIdRequirement
{
    public InstrumentViewerRequirement() : base()
    {
    }

    public InstrumentViewerRequirement(string id) : base(id)
    {
    }
}
