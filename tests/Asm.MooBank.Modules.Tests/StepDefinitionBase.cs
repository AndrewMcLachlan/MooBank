namespace Asm.MooBank.Modules.Tests;

[Binding]
internal class StepDefinitionBase
{
    internal Mocks Mocks { get; private set; }

    internal Models Models { get; private set; }

    internal Entities Entities { get; private set; }

    [BeforeScenario]
    public void BeforeScenario()
    {
        Mocks = new Mocks();
        Models = new Models();
        Entities = new Entities();
    }
}
