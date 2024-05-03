using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Commands;

namespace Asm.MooBank.Modules.Tests.Account.Commands.InstitutionAccount;

[Binding]
internal class UpdateStepDefinitions(ScenarioInput<Accounts.Models.Account.InstitutionAccount> input, ScenarioResult<Exception> exceptionResult) : StepDefinitionBase
{
    private Accounts.Models.Account.InstitutionAccount _result;

    [Given(@"I have a request to update an institution account")]
    public void GivenIHaveARequestToUpdateAnInstitutionAccount()
    {
        input.Value = Models.Account;
    }

    [When(@"I call UpdateHandler\.Handle")]
    public async Task WhenICallUpdateHandler_Handle()
    {
        Mock<IInstitutionAccountRepository> institutionAccountRepositoryMock = new();

        institutionAccountRepositoryMock.Setup(i => i.Get(Models.AccountId, new())).ReturnsAsync(
            new Domain.Entities.Account.InstitutionAccount(Models.AccountId)
            {
                Name = Models.Account.Name,
                //AccountController = Models.Account.AccountController,
                //AccountHolders = Models.Account.AccountHolders,
                AccountType = Models.Account.AccountType,
                //AccountViewers = Models.Account.AccountViewers,
                Balance = Models.Account.CurrentBalance,
                CalculatedBalance = Models.Account.CurrentBalance,
                Currency = Models.Account.Currency,
                Description = Models.Account.Description,
                //ImportAccount = Models.Account.ImportAccount,
                IncludeInBudget = Models.Account.IncludeInBudget,
                //Institution = Models.Account.Institution,
                InstitutionId = Models.Account.InstitutionId,
                LastTransaction = Models.Account.LastTransaction,
                //LastUpdated = Models.Account.LastUpdated,
                //Rules = Models.Account.Rules,
                ShareWithFamily = Models.Account.ShareWithFamily,
                //Slug = Models.Account.Slug,
                //Transactions = Models.Account.Transactions,
                //VirtualAccounts = Models.Account.VirtualAccounts,
            });

        UpdateHandler updateHandler = new(Mocks.UnitOfWorkMock.Object, institutionAccountRepositoryMock.Object, Models.AccountHolder, Mocks.CurrencyConverterMock.Object, Mocks.SecurityMock.Object);

        Update command = new(input.Value);

        await SpecFlowHelper.CatchExceptionAsync(async () => _result = await updateHandler.Handle(command, new CancellationToken()), exceptionResult);
    }

    [Then(@"the account is updated")]
    public void ThenTheAccountIsUpdated()
    {
        Assert.Fail();
    }

    [Given(@"I have an invalid account ID")]
    public void GivenIHaveAnInvalidAccountID()
    {
        input.Value.Id = Models.InvalidAccountId;
    }
}
