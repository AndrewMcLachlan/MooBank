using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Data.Repositories
{
    public interface IReferenceDataRepository
    {
        Task<IEnumerable<ImporterType>> GetImporterTypes();
    }
}
