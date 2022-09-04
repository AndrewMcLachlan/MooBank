using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Data.Repositories
{
    public interface IReferenceDataRepository
    {
        Task<IEnumerable<ImporterType>> GetImporterTypes();
    }
}
