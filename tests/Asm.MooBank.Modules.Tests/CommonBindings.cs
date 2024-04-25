namespace Asm.MooBank.Modules.Tests;

[Binding]
internal class CommonBindings(ScenarioInput<Asm.MooBank.Modules.Account.Models.Account.InstitutionAccount> scenarioInput)
{
    [Given(@"I have an invalid account group ID")]
    public void GivenIHaveAnInvalidAccountGroupID()
    {
        scenarioInput.Value.GroupId = Models.InvalidAccountGroupId;
    }

}
