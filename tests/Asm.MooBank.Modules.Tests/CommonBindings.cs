using Asm.MooBank.Modules.Accounts.Models.Account;

namespace Asm.MooBank.Modules.Tests;

[Binding]
internal class CommonBindings(ScenarioInput<InstitutionAccount> scenarioInput)
{
    [Given(@"I have an invalid account group ID")]
    public void GivenIHaveAnInvalidAccountGroupID()
    {
        scenarioInput.Value.GroupId = Models.InvalidAccountGroupId;
    }

}
