using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Domain.Entities.User.Specifications;
using Asm.MooBank.Modules.Users.Models;

namespace Asm.MooBank.Modules.Users.Commands;

internal record Update(UpdateUser User) : ICommand<Models.User>;

internal class UpdateHandler(IUnitOfWork unitOfWork, IUserRepository repository, MooBank.Models.User user) : ICommandHandler<Update, Models.User>
{
    public async ValueTask<Models.User> Handle(Update command, CancellationToken cancellationToken)
    {
        var entity = await repository.Get(user.Id, new GetWithCards(), cancellationToken);

        entity.Currency = command.User.Currency;
        entity.PrimaryAccountId = command.User.PrimaryAccountId;

        var existing = entity.Cards.Select(c => c.Last4Digits);
        var newCards = command.User.Cards.Select(c => c.Last4Digits);

        var delete = existing.Except(newCards);
        var add = newCards.Except(existing);
        var update = existing.Intersect(newCards);

        foreach (var card in delete)
        {
            entity.Cards.Remove(entity.Cards.Single(c => c.Last4Digits == card));
        }

        foreach (var card in add)
        {
            entity.Cards.Add(command.User.Cards.Select(c => new Domain.Entities.User.UserCard
            {
                UserId = user.Id,
                Name = c.Name,
                Last4Digits = c.Last4Digits,
            }).Single(c => c.Last4Digits == card));
        }

        foreach (var card in update)
        {
            var existingCard = entity.Cards.Single(c => c.Last4Digits == card);
            var newCard = command.User.Cards.Single(c => c.Last4Digits == card);

            existingCard.Name = newCard.Name;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
