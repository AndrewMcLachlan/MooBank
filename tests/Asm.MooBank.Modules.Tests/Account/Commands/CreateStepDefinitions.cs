using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Commands;

namespace Asm.MooBank.Modules.Tests.Account.Commands;

[Binding]
internal class CreateStepDefinitions(ScenarioContext context) : StepDefinitionBase
{
    private Accounts.Models.Account.LogicalAccount _result;

    [Given(@"I have a request to create an institution account")]
    public void GivenIHaveARequestToCreateAnInstitutionAccount()
    {
       context.Set(Models.Account);
    }

    [When(@"I call CreateHandler\.Handle")]
    public async Task WhenICallCreateHandler_Handle()
    {
        Mock<ILogicalAccountRepository> institutionAccountRepositoryMock = new();

        institutionAccountRepositoryMock.Setup(i => i.Add(It.IsAny<LogicalAccount>())).Returns(NewAccountEntity);
        institutionAccountRepositoryMock.Setup(i => i.Add(It.IsAny<LogicalAccount>(), It.IsAny<decimal>(), It.IsAny<DateTime>()))
            .Returns((LogicalAccount account, decimal balance) => NewAccountEntityWithBalance(account, balance));

        CreateHandler createHandler = new(institutionAccountRepositoryMock.Object, Mocks.UnitOfWorkMock.Object, Models.AccountHolder, Mocks.CurrencyConverterMock.Object, Mocks.SecurityMock.Object);

        Create command = new()
        {
            Balance = Models.Account.CurrentBalance,
            Currency = Models.Account.Currency,
            Description = Models.Account.Description,
            Name = Models.Account.Name,
            AccountType = Models.Account.AccountType,
            Controller = Models.Account.Controller,
            ShareWithFamily = Models.Account.ShareWithFamily,
            GroupId = Models.Account.GroupId,
            IncludeInBudget = Models.Account.IncludeInBudget,
            ImporterTypeId = Models.Account.InstitutionAccounts!.First().ImporterTypeId,
            InstitutionId = Models.Account.InstitutionAccounts!.First().InstitutionId,
        };

        await context.CatchExceptionAsync(async () => _result = await createHandler.Handle(command, new CancellationToken()));
    }

    [Then(@"the account is created")]
    public void ThenTheAccountIsCreated()
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

    private static LogicalAccount NewAccountEntity(LogicalAccount account) => NewAccountEntityWithBalance(account);
    private static LogicalAccount NewAccountEntityWithBalance(LogicalAccount account, decimal balance = 0) =>
         new(Models.AccountId, [])
         {
             Name = account.Name,
             Controller = account.Controller,
             Owners = account.Owners,
             AccountType = account.AccountType,
             Viewers = account.Viewers,
             Balance = balance,
             Currency = account.Currency,
             Description = account.Description,
             IncludeInBudget = account.IncludeInBudget,
             LastTransaction = account.LastTransaction,
             LastUpdated = account.LastUpdated,
             Rules = account.Rules,
             ShareWithFamily = account.ShareWithFamily,
             Slug = account.Slug,
             Transactions = account.Transactions,
             VirtualInstruments = account.VirtualInstruments,
         };
}
