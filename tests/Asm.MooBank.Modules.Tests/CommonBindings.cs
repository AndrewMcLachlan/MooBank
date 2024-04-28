using Asm.MooBank.Modules.Accounts.Models.Account;

namespace Asm.MooBank.Modules.Tests;

[Binding]
internal class CommonBindings(ScenarioInput<InstitutionAccount> scenarioInput)
{
    [Given(@"I have an invalid group ID")]
    public void GivenIHaveAnInvalidGroupId()
    {
        scenarioInput.Value.GroupId = Models.InvalidGroupId;
    }

}
