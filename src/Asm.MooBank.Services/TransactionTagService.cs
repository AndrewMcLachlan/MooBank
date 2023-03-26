using Asm.Domain;
using ITransactionTagRepository = Asm.MooBank.Domain.Entities.TransactionTags.ITransactionTagRepository;
using Asm.MooBank.Models;

namespace Asm.MooBank.Services;

public interface ITransactionTagService
{
    Task<TransactionTag> Create(TransactionTag tag);

    Task<TransactionTag> Create(string name);

    Task<TransactionTag> Update(int id, string name);

    Task<IEnumerable<TransactionTag>> GetAll(CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionTag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default);

    Task<TransactionTag> Get(int id, CancellationToken cancellationToken = default);

    Task Delete(int id);

    Task<TransactionTag> AddSubTag(int id, int subId);

    Task RemoveSubTag(int id, int subId);
}

public class TransactionTagService : ServiceBase, ITransactionTagService
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public TransactionTagService(IUnitOfWork unitOfWork, ITransactionTagRepository transactionTagRepository) : base(unitOfWork)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async Task<TransactionTag> Create(string name)
    {
        return await Create(new TransactionTag { Name = name });
    }

    public async Task<TransactionTag> Create(TransactionTag tag)
    {
        Domain.Entities.TransactionTags.TransactionTag transactionTag = tag;
        _transactionTagRepository.Add(transactionTag);

        await UnitOfWork.SaveChangesAsync();

        return transactionTag;
    }

    public async Task<TransactionTag> Update(int id, string name)
    {
        var tag = await GetEntity(id);

        tag.Name = name;

        await UnitOfWork.SaveChangesAsync();

        return tag;
    }

    public Task<IEnumerable<TransactionTag>> GetAll(CancellationToken cancellationToken = default) => _transactionTagRepository.GetAll().ToModelAsync(cancellationToken);
    //(await DataContext.TransactionTags.Include(t => t.Tags).Where(t => !t.Deleted).ToListAsync()).OrderBy(t => t.Name).Select(t => (TransactionTag)t).ToList();

    public Task<IEnumerable<TransactionTag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default) => _transactionTagRepository.Get(tagIds).ToModelAsync(cancellationToken);
    //return await DataContext.TransactionTags.Where(t => tagIds.Contains(t.TransactionTagId)).ToListAsync();

    public async Task<TransactionTag> Get(int id, CancellationToken cancellationToken = default) => await GetEntity(id, false, cancellationToken);

    public async Task Delete(int id)
    {
        var entity = await GetEntity(id);
        _transactionTagRepository.Delete(entity);

        await UnitOfWork.SaveChangesAsync();
    }

    public async Task<TransactionTag> AddSubTag(int id, int subId)
    {
        if (id == subId) throw new InvalidOperationException("Cannot add a tag to itself");

        var tag = await GetEntity(id, true);
        var subTag = await GetEntity(subId);

        if (tag.Tags.Any(t => t == subTag)) throw new ExistsException($"Tag with id {subId} has already been added");

        tag.Tags.Add(subTag);

        await UnitOfWork.SaveChangesAsync();

        return tag;
    }

    public async Task RemoveSubTag(int id, int subId)
    {
        var tag = await GetEntity(id, true);
        var subTag = await GetEntity(subId);

        if (!tag.Tags.Any(t => t == subTag)) throw new NotFoundException($"Tag with id {subId} has not been added");

        tag.Tags.Remove(subTag);

        await UnitOfWork.SaveChangesAsync();
    }

    private Task<Domain.Entities.TransactionTags.TransactionTag> GetEntity(int id, bool includeSubTags = false, CancellationToken cancellationToken = default) => _transactionTagRepository.Get(id, includeSubTags);
}
