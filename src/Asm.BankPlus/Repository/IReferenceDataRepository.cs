using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Repository
{
    public interface IReferenceDataRepository
    {
        Task<IEnumerable<ImporterType>> GetImporterTypes();
    }
}
