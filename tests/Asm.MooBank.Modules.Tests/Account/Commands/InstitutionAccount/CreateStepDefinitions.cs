using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Commands.InstitutionAccount;
using Asm.Testing;

namespace Asm.MooBank.Modules.Tests.Account.Commands.InstitutionAccount;

[Binding]
internal class CreateStepDefinitions(ScenarioInput<Accounts.Models.Account.InstitutionAccount> input, ScenarioResult<Exception> exceptionResult) : StepDefinitionBase
{
    private Accounts.Models.Account.InstitutionAccount _result;

    [Given(@"I have a request to create an institution account")]
    public void GivenIHaveARequestToCreateAnInstitutionAccount()
    {
        input.Value = Models.Account;
    }

    [When(@"I call CreateHandler\.Handle")]
    public async Task WhenICallCreateHandler_Handle()
    {
        Mock<IInstitutionAccountRepository> institutionAccountRepositoryMock = new();

        institutionAccountRepositoryMock.Setup(i => i.Add(It.IsAny<Domain.Entities.Account.InstitutionAccount>())).Returns<Domain.Entities.Account.InstitutionAccount>(account =>
            new Domain.Entities.Account.InstitutionAccount(Models.AccountId)
            {
                Name = account.Name,
                AccountController = account.AccountController,
                Owners = account.Owners,
                AccountType = account.AccountType,
                Viewers = account.Viewers,
                Balance = account.Balance,
                CalculatedBalance = account.Balance,
                Currency = account.Currency,
                Description = account.Description,
                ImportAccount = account.ImportAccount,
                IncludeInBudget = account.IncludeInBudget,
                Institution = account.Institution,
                IncludeInPosition = account.IncludeInPosition,
                InstitutionId = account.InstitutionId,
                LastTransaction = account.LastTransaction,
                LastUpdated = account.LastUpdated,
                Rules = account.Rules,
                ShareWithFamily = account.ShareWithFamily,
                Slug = account.Slug,
                Transactions = account.Transactions,
                VirtualInstruments = account.VirtualInstruments,
            });

        CreateHandler createHandler = new(institutionAccountRepositoryMock.Object, Mocks.UnitOfWorkMock.Object, Models.AccountHolder, Mocks.CurrencyConverterMock.Object, Mocks.SecurityMock.Object);

        Create command = new(input.Value);

        await SpecFlowHelper.CatchExceptionAsync(async () => _result = await createHandler.Handle(command, new CancellationToken()), exceptionResult);
    }

    [Then(@"the account is created")]
    public void ThenTheAccountIsCreated()
    {
        Assert.Equal(input.Value.Currency, _result.Currency);
        Assert.Equal(input.Value.CurrentBalance, _result.CurrentBalance);
        Assert.Equal(input.Value.CurrentBalanceLocalCurrency, _result.CurrentBalanceLocalCurrency);
        Assert.Equal(Models.AccountId, _result.Id);
        Assert.Equal(input.Value.Name, _result.Name);
        Assert.Equal(input.Value.AccountType, _result.AccountType);
        Assert.Equal(input.Value.Description, _result.Description);
        Assert.Equal(input.Value.LastTransaction, _result.LastTransaction);
        Assert.Equal(input.Value.AccountType, _result.AccountType);
        Assert.Equal(input.Value.Controller, _result.Controller);
        Assert.Equal(input.Value.ImporterTypeId, _result.ImporterTypeId);
        Assert.Equal(input.Value.ShareWithFamily, _result.ShareWithFamily);
        Assert.Equal(input.Value.IncludeInBudget, _result.IncludeInBudget);
    }
}
