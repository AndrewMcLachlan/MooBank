using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Domain.Entities.User.Specifications;
using Asm.MooBank.Modules.Users.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace Asm.MooBank.Modules.Users.Commands;

internal record Update(UpdateUser User) : ICommand<Models.User>;

internal class UpdateHandler(IUnitOfWork unitOfWork, IUserRepository repository, MooBank.Models.User user, HybridCache cache) : ICommandHandler<Update, Models.User>
{
    public async ValueTask<Models.User> Handle(Update command, CancellationToken cancellationToken)
    {
        var entity = await repository.Get(user.Id, new GetWithCards(), cancellationToken);

        entity.Currency = command.User.Currency;
        entity.PrimaryAccountId = command.User.PrimaryAccountId;

        var existing = entity.Cards.Select(c => c.Last4Digits).ToList();
        var newCards = command.User.Cards.Select(c => c.Last4Digits).ToList();

        var delete = existing.Except(newCards);
        var add = newCards.Except(existing);
        var update = existing.Intersect(newCards);

        foreach (var card in delete)
        {
            entity.Cards.Remove(entity.Cards.Single(c => c.Last4Digits == card));
        }

        foreach (var card in add)
        {
            var newCard = command.User.Cards.Single(c => c.Last4Digits == card);

            entity.Cards.Add(new Domain.Entities.User.UserCard
            {
                UserId = user.Id,
                Name = newCard.Name,
                Last4Digits = newCard.Last4Digits,
            });
        }

        foreach (var card in update)
        {
            entity.Cards.Single(c => c.Last4Digits == card).Name = command.User.Cards.Single(c => c.Last4Digits == card).Name;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(CacheKeys.UserCacheKey(user.Id), cancellationToken);

        return entity.ToModel();
    }
}
