namespace Asm.MooBank.Models;

public record ImportWorkItem(Guid InstrumentId, Guid AccountId, User User, byte[] FileData);
