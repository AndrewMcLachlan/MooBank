using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferenceDataController : ControllerBase
    {
        private readonly IReferenceDataRepository _repository;

        public ReferenceDataController(IReferenceDataRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("importertypes")]
        public async Task<ActionResult<IEnumerable<ImporterType>>> GetImporterTypes()
        {
            return Ok(await _repository.GetImporterTypes());
        }
    }
}
