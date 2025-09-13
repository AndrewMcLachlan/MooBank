using Asm.Drawing;

namespace Asm.MooBank.Modules.Tags.Models;
public record UpdateTag(string Name, HexColour Colour, bool ExcludeFromReporting, bool ApplySmoothing);
