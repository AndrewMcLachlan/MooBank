namespace Asm.BankPlus.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransactionsController : ControllerBase
{
    private ITransactionRepository TransactionRepository { get; }

    public TransactionsController(ITransactionRepository transactionRepository)
    {
        TransactionRepository = transactionRepository;
    }

    [HttpPut("{id}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> Add(Guid id, int tagId)
    {
        return Created($"api/transactions/{id}/tag/{tagId}",
            await TransactionRepository.AddTransactionTag(id, tagId));
    }

    [HttpDelete("{id}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> RemoveTag(Guid id, int tagId)
    {
        return await TransactionRepository.RemoveTransactionTag(id, tagId);
    }
}
