using Asm.Domain;
using Asm.MooBank.Domain.Entities.TransactionTags;
using Asm.MooBank.Models;
using Asm.MooBank.Models.Commands.TransactionTags;

namespace Asm.MooBank.Services.Commands.TransactionTags;

internal class UpdateHandler : ICommandHandler<Update, Models.TransactionTag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHandler(ITransactionTagRepository transactionTagRepository, IUnitOfWork unitOfWork)
    {
        _transactionTagRepository = transactionTagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.TransactionTag> Handle(Update request, CancellationToken cancellationToken)
    {
        var tag = await _transactionTagRepository.Get(request.TagId, cancellationToken);
        tag.Name = request.Name;
        tag.Settings.ExcludeFromReporting = request.ExcludeFromReporting;
        tag.Settings.ApplySmoothing = request.ApplySmoothing;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
