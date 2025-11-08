using Asm.MooBank.Modules.Accounts.Models.Account;

namespace Asm.MooBank.Modules.Tests;

[Binding]
internal class CommonBindings(ScenarioContext context)
{
    [Given(@"I have an invalid group ID")]
    public void GivenIHaveAnInvalidGroupId()
    {
        context.Get<LogicalAccount>().GroupId = Models.InvalidGroupId;
    }

    [Given(@"I have an invalid account ID")]
    public void GivenIHaveAnInvalidAccountID()
    {
        context.Get<LogicalAccount>().Id = Models.InvalidAccountId;
    }
}
