using Asm.BankPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Services
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
