using Asm.Domain;
using Asm.MooBank.Models;
using ITransactionTagRepository = Asm.MooBank.Domain.Entities.TransactionTags.ITransactionTagRepository;

namespace Asm.MooBank.Services;

public interface ITransactionTagService
{
    Task<IEnumerable<TransactionTag>> GetAll(CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionTag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default);

    Task<TransactionTag> Get(int id, CancellationToken cancellationToken = default);

    Task Delete(int id);

    Task RemoveSubTag(int id, int subId);
}

public class TransactionTagService : ServiceBase, ITransactionTagService
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public TransactionTagService(IUnitOfWork unitOfWork, ITransactionTagRepository transactionTagRepository) : base(unitOfWork)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async Task<IEnumerable<TransactionTag>> GetAll(CancellationToken cancellationToken = default) => (await _transactionTagRepository.GetAll(cancellationToken).ToModelAsync(cancellationToken)).OrderBy(t => t.Name);
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
