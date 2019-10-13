using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asm.BankPlus.Data;
using Asm.BankPlus.Importers;
using Asm.BankPlus.Repository;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Services
{
    public class ImporterFactory : DataRepository, IImporterFactory
    {
        private readonly IServiceProvider _services;

        public ImporterFactory(BankPlusContext context, IServiceProvider services) : base(context)
        {
            _services = services;
        }

        public IImporter Create(Guid accountId)
        {
            var account = DataContext.ImportAccounts.Include(i => i.ImporterType).Single(i => i.AccountId == accountId);

            return _services.GetService(Type.GetType(account.ImporterType.Type)) as IImporter;
        }
    }
}
