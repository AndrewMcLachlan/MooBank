using Asm.Drawing;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Asm.MooBank.Infrastructure.ValueConverters;

public class HexColourConverter : ValueConverter<HexColour?, string?>
{
    public HexColourConverter() : base(
        v => v.HasValue ? v.Value.HexString : null,
        v => String.IsNullOrEmpty(v) ? null : new HexColour(v))
    {
    }
}
