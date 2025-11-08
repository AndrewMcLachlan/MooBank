using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Modules.Accounts.Commands;

namespace Asm.MooBank.Modules.Tests.Account.Commands;

[Binding]
internal class UpdateStepDefinitions(ScenarioContext context) : StepDefinitionBase
{
    private Accounts.Models.Account.LogicalAccount _result;

    [Given(@"I have a request to update an institution account")]
    public void GivenIHaveARequestToUpdateAnInstitutionAccount()
    {
        context.Set(Models.Account);
    }

    [When(@"I call UpdateHandler\.Handle")]
    public async Task WhenICallUpdateHandler_Handle()
    {
        Mock<ILogicalAccountRepository> institutionAccountRepositoryMock = new();

        institutionAccountRepositoryMock.Setup(i => i.Get(Models.AccountId, It.IsAny<AccountDetailsSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(
            new Domain.Entities.Account.LogicalAccount(Models.AccountId, [])
            {
                Name = Models.Account.Name,
                //AccountController = Models.Account.AccountController,
                //AccountHolders = Models.Account.AccountHolders,
                AccountType = Models.Account.AccountType,
                //AccountViewers = Models.Account.AccountViewers,
                Balance = Models.Account.CurrentBalance,
                Currency = Models.Account.Currency,
                Description = Models.Account.Description,
                IncludeInBudget = Models.Account.IncludeInBudget,
                LastTransaction = Models.Account.LastTransaction,
                //LastUpdated = Models.Account.LastUpdated,
                //Rules = Models.Account.Rules,
                ShareWithFamily = Models.Account.ShareWithFamily,
                //Slug = Models.Account.Slug,
                //Transactions = Models.Account.Transactions,
                //VirtualAccounts = Models.Account.VirtualAccounts,
            });

        UpdateHandler updateHandler = new(Mocks.UnitOfWorkMock.Object, institutionAccountRepositoryMock.Object, Models.AccountHolder, Mocks.CurrencyConverterMock.Object, Mocks.SecurityMock.Object);

        Update command = new(Models.Account);

        await context.CatchExceptionAsync(async () => _result = await updateHandler.Handle(command, new CancellationToken()));
    }

    [Then(@"the account is updated")]
    public void ThenTheAccountIsUpdated()
    {
        Assert.NotNull(_result);
        Assert.Equal(Models.Account.Currency, _result.Currency);
        Assert.Equal(Models.Account.CurrentBalance, _result.CurrentBalance);
        Assert.Equal(Models.Account.CurrentBalanceLocalCurrency, _result.CurrentBalanceLocalCurrency);
        Assert.Equal(Models.Account.Id, _result.Id);
        Assert.Equal(Models.Account.Name, _result.Name);
        Assert.Equal(Models.Account.Description, _result.Description);
        Assert.Equal(Models.Account.LastTransaction, _result.LastTransaction);
        Assert.Equal(Models.Account.InstrumentType, _result.InstrumentType);
        Assert.Equal(Models.Account.Controller, _result.Controller);
        Assert.Equal(Models.Account.ShareWithFamily, _result.ShareWithFamily);
        Assert.Equal(Models.Account.IncludeInBudget, _result.IncludeInBudget);
    }
}
