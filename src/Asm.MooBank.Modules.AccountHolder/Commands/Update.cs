﻿using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.AccountHolder.Specifications;
using Asm.MooBank.Modules.Users.Models;

namespace Asm.MooBank.Modules.Users.Commands;

internal record Update(UpdateAccountHolder AccountHolder) : ICommand<AccountHolder>;

internal class UpdateHandler(IUnitOfWork unitOfWork, IAccountHolderRepository repository, MooBank.Models.User user) : ICommandHandler<Update, AccountHolder>
{
    public async ValueTask<AccountHolder> Handle(Update command, CancellationToken cancellationToken)
    {
        var entity = await repository.Get(user.Id, new GetWithCards(), cancellationToken);

        entity.Currency = command.AccountHolder.Currency;
        entity.PrimaryAccountId = command.AccountHolder.PrimaryAccountId;

        var existing = entity.Cards.Select(c => c.Last4Digits);
        var newCards = command.AccountHolder.Cards.Select(c => c.Last4Digits);

        var delete = existing.Except(newCards);
        var add = newCards.Except(existing);
        var update = existing.Intersect(newCards);

        foreach (var card in delete)
        {
            entity.Cards.Remove(entity.Cards.Single(c => c.Last4Digits == card));
        }

        foreach (var card in add)
        {
            entity.Cards.Add(command.AccountHolder.Cards.Select(c => new Domain.Entities.AccountHolder.AccountHolderCard
            {
                AccountHolderId = user.Id,
                Name = c.Name,
                Last4Digits = c.Last4Digits,
            }).Single(c => c.Last4Digits == card));
        }

        foreach (var card in update)
        {
            var existingCard = entity.Cards.Single(c => c.Last4Digits == card);
            var newCard = command.AccountHolder.Cards.Single(c => c.Last4Digits == card);

            existingCard.Name = newCard.Name;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
