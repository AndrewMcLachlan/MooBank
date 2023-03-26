using Asm.Domain;
using ITransactionRepository = Asm.MooBank.Domain.Entities.Transactions.ITransactionRepository;
using Asm.MooBank.Domain.Entities.TransactionTags;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Services;

public interface ITransactionService
{
    Task<int> GetTotalTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default);

    Task<int> GetTotalUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);

    Task<Transaction> AddTransactionTag(Guid id, int tagId, CancellationToken cancellationToken = default);

    Task<Transaction> AddTransactionTags(Guid id, IEnumerable<int> tags, CancellationToken cancellationToken = default);

    Task<Transaction> RemoveTransactionTag(Guid id, int tagId, CancellationToken cancellationToken = default);

    Task AddTransaction(decimal amount, Guid accountId, bool isRecurring, string? description = null);
}

public class TransactionService : ServiceBase, ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionTagRepository _transactionTagRepository;
    private readonly ISecurityRepository _security;


    public TransactionService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, ITransactionTagRepository transactionTagRepository, ISecurityRepository securityRepository) : base(unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _transactionTagRepository = transactionTagRepository;
        _security = securityRepository;
    }

    public async Task<int> GetTotalTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionRepository.GetTransactionCount(accountId, filter, start, end, cancellationToken);

    }

    public async Task<int> GetTotalUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionRepository.GetUntaggedTransactionCount(accountId, filter, start, end, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return (await _transactionRepository.GetTransactions(accountId)).Select(t => (Transaction)t).OrderByDescending(t => t.TransactionTime);
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionRepository.GetTransactions(accountId, filter, start, end, pageSize, pageNumber, sortField, sortDirection).ToModelAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionRepository.GetTransactions(accountId, start, end, pageSize, pageNumber, sortField, sortDirection).ToModelAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionRepository.GetTransactions(accountId, period, pageSize, pageNumber, sortField, sortDirection).ToModelAsync();
    }

    public async Task<IEnumerable<Transaction>> GetUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default)
    {
        _security.AssertAccountPermission(accountId);

        return await _transactionRepository.GetTransactions(accountId, filter, start, end, pageSize, pageNumber, sortField, sortDirection).ToModelAsync();
    }

    public async Task<Transaction> AddTransactionTag(Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntity(id);

        if (entity.TransactionTags.Any(t => t.TransactionTagId == tagId)) throw new ExistsException("Cannot add tag, it already exists");

        entity.TransactionTags.Add(entity.TransactionTags.Single(t => t.TransactionTagId == tagId));

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return (Transaction)entity;
    }

    public async Task<Transaction> AddTransactionTags(Guid id, IEnumerable<int> tags, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntity(id);

        var existingIds = entity.TransactionTags.Select(t => t.TransactionTagId);

        var filteredTags = tags.Where(t => !existingIds.Contains(t));

        var tagEntities = await _transactionTagRepository.Get(filteredTags);

        entity.TransactionTags.AddRange(tagEntities);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return (Transaction)entity;
    }

    public async Task<Transaction> RemoveTransactionTag(Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntity(id);

        var tag = entity.TransactionTags.SingleOrDefault(t => t.TransactionTagId == tagId);

        if (tag == null) throw new NotFoundException("Tag not found");

        entity.TransactionTags.Remove(tag);

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


    public async Task AddTransaction(decimal amount, Guid accountId, bool isRecurring, string? description = null)
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

        await UnitOfWork.SaveChangesAsync();
    }
}
