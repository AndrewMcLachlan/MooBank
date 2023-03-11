using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services
{
    public class TransactionTagRepository : DataRepository, ITransactionTagRepository
    {
        public TransactionTagRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public async Task<TransactionTag> Create(string name)
        {
            return await Create(new TransactionTag { Name = name });
        }

        public async Task<TransactionTag> Create(TransactionTag tag)
        {
            Data.Entities.TransactionTag transactionTag = tag;
            DataContext.Add(transactionTag);

            await DataContext.SaveChangesAsync();

            return await Get(transactionTag.TransactionTagId);
        }

        public async Task<TransactionTag> Update(int id, string name)
        {
            var tag = await GetEntity(id);

            tag.Name = name;

            await DataContext.SaveChangesAsync();

            return tag;
        }

        public async Task<IEnumerable<TransactionTag>> Get()
        {
            return (await DataContext.TransactionTags.Include(t => t.Tags).Where(t => !t.Deleted).ToListAsync()).OrderBy(t => t.Name).Select(t => (TransactionTag)t).ToList();
        }

        public async Task<IEnumerable<Data.Entities.TransactionTag>> Get(IEnumerable<int> tagIds)
        {
            return await DataContext.TransactionTags.Where(t => tagIds.Contains(t.TransactionTagId)).ToListAsync();
        }

        public async Task<TransactionTag> Get(int id)
        {
            return await GetEntity(id);
        }

        public async Task Delete(int id)
        {
            var tag = await GetEntity(id);

            tag.Deleted = true;

            await DataContext.SaveChangesAsync();
        }

        public async Task<TransactionTag> AddSubTag(int id, int subId)
        {
            if (id == subId) throw new InvalidOperationException("Cannot add a tag to itself");

            var tag = await GetEntity(id, true);
            var subTag = await GetEntity(subId);

            if (tag.Tags.Any(t => t == subTag)) throw new ExistsException($"Tag with id {subId} has already been added");

            tag.Tags.Add(subTag);

            await DataContext.SaveChangesAsync();

            return tag;
        }

        public async Task RemoveSubTag(int id, int subId)
        {
            var tag = await GetEntity(id, true);
            var subTag = await GetEntity(subId);

            if (!tag.Tags.Any(t => t == subTag)) throw new NotFoundException($"Tag with id {subId} has not been added");

            tag.Tags.Remove(subTag);

            await DataContext.SaveChangesAsync();
        }

        private async Task<Data.Entities.TransactionTag> GetEntity(int id, bool includeSubTags = false)
        {
            var tag = includeSubTags ?
                await DataContext.TransactionTags.Include(t => t.Tags).Where(c => c.TransactionTagId == id).SingleOrDefaultAsync() :
                await DataContext.TransactionTags.Where(c => c.TransactionTagId == id).SingleOrDefaultAsync();

            if (tag == null) throw new NotFoundException($"Transaction tag with id {id} was not found");

            return tag;
        }
    }
}
