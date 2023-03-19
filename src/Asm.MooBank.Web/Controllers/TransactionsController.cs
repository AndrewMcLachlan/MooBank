namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPut("{id}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> Add(Guid id, int tagId, CancellationToken cancellationToken = default)
    {
        return Created($"api/transactions/{id}/tag/{tagId}", await _transactionService.AddTransactionTag(id, tagId, cancellationToken));
    }

    [HttpDelete("{id}/tag/{tagId}")]
    public async Task<ActionResult<Transaction>> RemoveTag(Guid id, int tagId)
    {
        return await _transactionService.RemoveTransactionTag(id, tagId);
    }
}
