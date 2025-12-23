using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Domain.Entities.User;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Families.Commands;

[DisplayName("RemoveFamilyMember")]
public sealed record RemoveMember([FromRoute] Guid UserId) : ICommand;

internal class RemoveMemberHandler(
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    MooBank.Models.User currentUser) : ICommandHandler<RemoveMember>
{
    public async ValueTask Handle(RemoveMember command, CancellationToken cancellationToken)
    {
        if (command.UserId == currentUser.Id)
        {
            throw new InvalidOperationException("You cannot remove yourself from the family.");
        }

        var family = await familyRepository.Get(currentUser.FamilyId, cancellationToken);

        var memberToRemove = await userRepository.Get(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (memberToRemove.FamilyId != currentUser.FamilyId)
        {
            throw new InvalidOperationException("User is not a member of your family.");
        }

        // Check if this would leave the family empty (shouldn't happen since we can't remove ourselves)
        if (family.AccountHolders.Count <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last member of a family.");
        }

        // Create a new family for the removed member
        var newFamily = new Domain.Entities.Family.Family
        {
            Name = $"{memberToRemove.FirstName ?? memberToRemove.EmailAddress}'s Family"
        };
        familyRepository.Add(newFamily);

        // Move the user to their new family
        memberToRemove.FamilyId = newFamily.Id;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
