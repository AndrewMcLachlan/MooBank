using System;

namespace Asm.MooBank.Importers
{
    public interface IImporterFactory
    {
        IImporter Create(Guid accountId);
    }
}
