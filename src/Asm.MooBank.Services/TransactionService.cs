using Asm.Domain;
using ITransactionRepository = Asm.MooBank.Domain.Entities.Transactions.ITransactionRepository;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;

namespace Asm.MooBank.Services;

public interface ITransactionService
{
    Task<Transaction> AddTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default);

    Task<Transaction> RemoveTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default);

    void AddTransaction(decimal amount, Guid accountId, bool isRecurring, string? description = null);
}

public class TransactionService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, ITagRepository transactionTagRepository, ISecurity securityRepository) : ServiceBase(unitOfWork), ITransactionService
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ITagRepository _transactionTagRepository = transactionTagRepository;
    private readonly ISecurity _security = securityRepository;

    public async Task<Transaction> AddTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        var entity = await GetEntity(id);

        if (entity.Tags.Any(t => t.Id == tagId)) throw new ExistsException("Cannot add tag, it already exists");

        var tag = await _transactionTagRepository.Get(tagId, cancellationToken);

        entity.AddOrUpdateSplit(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    public async Task<Transaction> RemoveTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        var entity = await GetEntity(id);

        var tag = entity.Tags.SingleOrDefault(t => t.Id == tagId) ?? throw new NotFoundException("Tag not found");

        entity.UpdateOrRemoveSplit(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    /*public async Task<IEnumerable<Transaction>> CreateTransactions(IEnumerable<Transaction> transactions)
    {
        transactions.Select(t => t.AccountId).Distinct().ToList().ForEach(_security.AssertAccountPermission);

        var entities = transactions.Select(t => (Domain.Entities.Transactions.Transaction)t).ToList();

        _transactionRepository.AddRange(entities);

        await UnitOfWork.SaveChangesAsync();

        return entities.Select(t => (Transaction)t);
    }*/

    private async Task<Domain.Entities.Transactions.Transaction> GetEntity(Guid id)
    {
        Domain.Entities.Transactions.Transaction entity = await _transactionRepository.Get(id);

        _security.AssertAccountPermission(entity.AccountId);

        return entity;
    }


    public void AddTransaction(decimal amount, Guid accountId, bool isRecurring, string? description = null)
    {
        TransactionType transactionType = amount < 0 ?
                                          isRecurring ? TransactionType.RecurringDebit : TransactionType.Debit :
                                          isRecurring ? TransactionType.RecurringCredit : TransactionType.Credit;

        Domain.Entities.Transactions.Transaction transaction = new()
        {
            Amount = amount,
            AccountId = accountId,
            Description = description,
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = transactionType,
        };

        _transactionRepository.Add(transaction);
    }
}
