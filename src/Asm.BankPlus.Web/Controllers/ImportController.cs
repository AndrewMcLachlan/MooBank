using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Importers;
using Asm.BankPlus.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IImporterFactory _importerFactory;

        public ImportController(IAccountRepository accountRepository, IImporterFactory importerFactory)
        {
            _importerFactory = importerFactory;
            _accountRepository = accountRepository;
        }

        [Route("api/accounts/{accountid}/[controller]")]
        public async Task Post(Guid accountId, IFormFile file)
        {
            var account = await _accountRepository.GetAccount(accountId);

            IImporter importer = _importerFactory.Create(accountId);

            await importer.Import(account, file.OpenReadStream());

            return NoContent();
        }

    }
}