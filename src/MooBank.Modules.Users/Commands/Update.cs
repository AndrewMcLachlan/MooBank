using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Domain.Entities.User.Specifications;
using Asm.MooBank.Modules.Users.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;

namespace Asm.MooBank.Modules.Users.Commands;

internal record Update(UpdateUser User) : ICommand<Models.User>;

internal class UpdateHandler(IUnitOfWork unitOfWork, IUserRepository repository, MooBank.Models.User user, HybridCache cache) : ICommandHandler<Update, Models.User>
{
    public async ValueTask<Models.User> Handle(Update command, CancellationToken cancellationToken)
    {
        var duplicates = command.User.Cards.GroupBy(c => c.Last4Digits).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        if (duplicates.Count != 0)
        {
            throw new BadHttpRequestException($"Duplicate card numbers supplied: {String.Join(", ", duplicates)}");
        }

        var entity = await repository.Get(user.Id, new GetWithCards(), cancellationToken);

        entity.Currency = command.User.Currency;
        entity.PrimaryAccountId = command.User.PrimaryAccountId;

        var existing = entity.Cards.Select(c => c.Last4Digits).Distinct().ToList();
        var newCards = command.User.Cards.Select(c => c.Last4Digits).ToList();

        var delete = existing.Except(newCards).ToList();
        var add = newCards.Except(existing).ToList();
        var update = existing.Intersect(newCards).ToList();

        foreach (var card in delete)
        {
            // Remove all matching cards, in case existing data contains duplicates.
            foreach (var existingCard in entity.Cards.Where(c => c.Last4Digits == card).ToList())
            {
                entity.Cards.Remove(existingCard);
            }
        }

        foreach (var card in add)
        {
            var newCard = command.User.Cards.First(c => c.Last4Digits == card);

            entity.Cards.Add(new Domain.Entities.User.UserCard
            {
                UserId = user.Id,
                Name = newCard.Name,
                Last4Digits = newCard.Last4Digits,
            });
        }

        foreach (var card in update)
        {
            var newCard = command.User.Cards.First(c => c.Last4Digits == card);

            // Update all matching cards, in case existing data contains duplicates.
            foreach (var existingCard in entity.Cards.Where(c => c.Last4Digits == card))
            {
                existingCard.Name = newCard.Name;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(CacheKeys.UserCacheKey(user.Id), cancellationToken);

        return entity.ToModel();
    }
}
