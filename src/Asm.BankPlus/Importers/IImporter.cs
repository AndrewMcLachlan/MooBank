using System.IO;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Importers
{
    public interface IImporter
    {
        Task<TransactionImportResult> Import(Account account, Stream contents);
    }
}
