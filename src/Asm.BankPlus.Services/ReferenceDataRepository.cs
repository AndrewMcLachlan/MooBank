using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
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
