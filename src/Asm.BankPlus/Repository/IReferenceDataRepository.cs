using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Repository
{
    public interface IReferenceDataRepository
    {
        Task<IEnumerable<ImporterType>> GetImporterTypes();
    }
}
