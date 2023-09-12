using Asm.Domain;
using ITransactionRepository = Asm.MooBank.Domain.Entities.Transactions.ITransactionRepository;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Services;

public interface ITransactionService
{
    Task<Transaction> AddTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default);

    Task<Transaction> AddTransactionTags(Guid id, IEnumerable<int> tags, CancellationToken cancellationToken = default);

    Task<Transaction> RemoveTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default);

    void AddTransaction(decimal amount, Guid accountId, bool isRecurring, string? description = null);
}

public class TransactionService : ServiceBase, ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionTagRepository _transactionTagRepository;
    private readonly ISecurity _security;


    public TransactionService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, ITransactionTagRepository transactionTagRepository, ISecurity securityRepository) : base(unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _transactionTagRepository = transactionTagRepository;
        _security = securityRepository;
    }



    public async Task<Transaction> AddTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        var entity = await GetEntity(id);

        if (entity.Tags.Any(t => t.Id == tagId)) throw new ExistsException("Cannot add tag, it already exists");

        var tag = await _transactionTagRepository.Get(tagId);

        entity.Tags.Add(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return (Transaction)entity;
    }

    public async Task<Transaction> AddTransactionTags(Guid id, IEnumerable<int> tags, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntity(id);

        var existingIds = entity.Tags.Select(t => t.Id);

        var filteredTags = tags.Where(t => !existingIds.Contains(t));

        var tagEntities = await _transactionTagRepository.Get(filteredTags);

        entity.Tags.AddRange(tagEntities);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return (Transaction)entity;
    }

    public async Task<Transaction> RemoveTransactionTag(Guid accountId, Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        var entity = await GetEntity(id);

        var tag = entity.Tags.SingleOrDefault(t => t.Id == tagId);

        if (tag == null) throw new NotFoundException("Tag not found");

        entity.Tags.Remove(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return (Transaction)entity;
    }

    public async Task<IEnumerable<Transaction>> CreateTransactions(IEnumerable<Transaction> transactions)
    {
        transactions.Select(t => t.AccountId).Distinct().ToList().ForEach(_security.AssertAccountPermission);

        var entities = transactions.Select(t => (Domain.Entities.Transactions.Transaction)t).ToList();

        _transactionRepository.AddRange(entities);

        await UnitOfWork.SaveChangesAsync();

        return entities.Select(t => (Transaction)t);
    }

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
            TransactionTime = DateTime.UtcNow,
            TransactionType = transactionType,
        };

        _transactionRepository.Add(transaction);
    }
}
