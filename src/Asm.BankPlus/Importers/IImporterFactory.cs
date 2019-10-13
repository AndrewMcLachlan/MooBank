using System;

namespace Asm.BankPlus.Importers
{
    public interface IImporterFactory
    {
        IImporter Create(Guid accountId);
    }
}
