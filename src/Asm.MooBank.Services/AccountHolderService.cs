using Asm.Domain;
using IAccountHolderRepository = Asm.MooBank.Domain.Entities.AccountHolder.IAccountHolderRepository;
using Asm.MooBank.Models;
using Asm.MooBank.Security;

namespace Asm.MooBank.Services
{
    public interface IAccountHolderService
    {
        Task<AccountHolder> GetCurrent(CancellationToken cancellationToken = default);
    }

    public class AccountHolderService : ServiceBase, IAccountHolderService
    {
        private readonly IAccountHolderRepository _accountHolderRepository;
        private readonly IUserDataProvider _userDataProvider;

        public AccountHolderService(IUnitOfWork unitOfWork, IAccountHolderRepository accountHolderRepository, IUserDataProvider userDataProvider) : base(unitOfWork)
        {
            _accountHolderRepository = accountHolderRepository;
            _userDataProvider = userDataProvider;
        }

        public async Task<AccountHolder> GetCurrent(CancellationToken cancellationToken = default)
        {
            var accountHolder = await _accountHolderRepository.GetCurrentOrNull(cancellationToken);

            if (accountHolder != null)
            {
                return accountHolder.ToModel();
            }

            accountHolder = (await _userDataProvider.GetCurrentUserAsync(cancellationToken)).ToDomain();

            _accountHolderRepository.Add(accountHolder);

            await UnitOfWork.SaveChangesAsync(cancellationToken);

            return accountHolder.ToModel();
        }
    }
}
