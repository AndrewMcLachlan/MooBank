using System.IO;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Importers
{
    public interface IImporter
    {
        Task<TransactionImportResult> Import(Account account, Stream contents);
    }
}
