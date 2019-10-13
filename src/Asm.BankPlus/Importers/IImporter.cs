using System.IO;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Importers
{
    public interface IImporter
    {
        Task Import(Account account, Stream contents);
    }
}
