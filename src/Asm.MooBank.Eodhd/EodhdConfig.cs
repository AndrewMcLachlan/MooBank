using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.MooBank.Eodhd;
public record EodhdConfig
{
    public required string ApiKey { get; init; }

}
