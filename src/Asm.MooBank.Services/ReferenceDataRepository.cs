using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services
{
    public class ReferenceDataRepository : DataRepository, IReferenceDataRepository
    {
        public ReferenceDataRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public async Task<IEnumerable<ImporterType>> GetImporterTypes()
        {
            return await DataContext.ImporterTypes.Select(i => (ImporterType)i).ToListAsync();
        }
    }
}
